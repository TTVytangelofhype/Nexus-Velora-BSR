using System.IO.Compression;
using NexusVeloraBSR.Core.BeatSaver;

namespace NexusVeloraBSR.Bridge;

public sealed class BeatSaberMapInstaller
{
    private readonly IHttpClientFactory _clients;
    private readonly IConfiguration _config;
    private readonly ILogger<BeatSaberMapInstaller> _log;

    public BeatSaberMapInstaller(IHttpClientFactory clients, IConfiguration config, ILogger<BeatSaberMapInstaller> log)
    {
        _clients = clients;
        _config = config;
        _log = log;
    }

    public string? FindBeatSaberPath()
    {
        var configured = _config["NexusVeloraBSR:BeatSaberPath"];
        if (IsBeatSaberPath(configured)) return Path.GetFullPath(configured!);

        foreach (var steamRoot in FindSteamRoots())
        {
            var direct = Path.Combine(steamRoot, "steamapps", "common", "Beat Saber");
            if (IsBeatSaberPath(direct)) return Path.GetFullPath(direct);

            var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            foreach (var library in ReadSteamLibraryPaths(vdf))
            {
                var candidate = Path.Combine(library, "steamapps", "common", "Beat Saber");
                if (IsBeatSaberPath(candidate)) return Path.GetFullPath(candidate);
            }
        }

        var oculus = @"C:\Program Files\Oculus\Software\Software\hyperbolic-magnetism-beat-saber";
        return IsBeatSaberPath(oculus) ? oculus : null;
    }

    public async Task<string> InstallAsync(ResolvedBeatSaverMap map, CancellationToken ct = default)
    {
        if (map.DownloadUri == null || map.DownloadUri.Scheme != Uri.UriSchemeHttps ||
            !map.DownloadUri.Host.EndsWith("beatsaver.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("BeatSaver returned an invalid download URL.");

        var game = FindBeatSaberPath() ?? throw new DirectoryNotFoundException(
            "Beat Saber was not found. Set NexusVeloraBSR:BeatSaberPath in appsettings.json.");

        var customLevels = Path.Combine(game, "Beat Saber_Data", "CustomLevels");
        Directory.CreateDirectory(customLevels);

        var safeKey = Sanitize(map.Key);
        var safeName = Sanitize(map.Name);
        var destination = Path.Combine(customLevels, $"{safeKey} ({safeName})");
        if (Directory.Exists(destination) && File.Exists(Path.Combine(destination, "Info.dat")) ||
            Directory.Exists(destination) && File.Exists(Path.Combine(destination, "info.dat")))
            return destination;

        var tempRoot = Path.Combine(Path.GetTempPath(), "NexusVeloraBSR", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var zipPath = Path.Combine(tempRoot, "map.zip");
        var extractPath = Path.Combine(tempRoot, "extract");

        try
        {
            using var client = _clients.CreateClient("beatsaver-download");
            using var response = await client.GetAsync(map.DownloadUri, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var length = response.Content.Headers.ContentLength;
            if (length.HasValue && length.Value > 200L * 1024 * 1024)
                throw new InvalidDataException("Map ZIP exceeds the 200 MB safety limit.");

            await using (var input = await response.Content.ReadAsStreamAsync(ct))
            await using (var output = File.Create(zipPath))
                await input.CopyToAsync(output, ct);

            Directory.CreateDirectory(extractPath);
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                long total = 0;
                foreach (var entry in archive.Entries)
                {
                    total += entry.Length;
                    if (total > 500L * 1024 * 1024)
                        throw new InvalidDataException("Extracted map exceeds the 500 MB safety limit.");

                    var target = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                    var root = Path.GetFullPath(extractPath) + Path.DirectorySeparatorChar;
                    if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Unsafe path detected in map ZIP.");

                    if (string.IsNullOrEmpty(entry.Name)) Directory.CreateDirectory(target);
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                        entry.ExtractToFile(target, true);
                    }
                }
            }

            var levelRoot = FindLevelRoot(extractPath)
                ?? throw new InvalidDataException("Downloaded ZIP does not contain a Beat Saber Info.dat.");

            if (Directory.Exists(destination)) Directory.Delete(destination, true);
            CopyDirectory(levelRoot, destination);
            _log.LogInformation("Installed BeatSaver map {Key} to {Path}", map.Key, destination);
            return destination;
        }
        finally
        {
            try { if (Directory.Exists(tempRoot)) Directory.Delete(tempRoot, true); } catch { }
        }
    }

    private static IEnumerable<string> FindSteamRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Add(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)) roots.Add(Path.GetFullPath(path));
        }

        Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
        Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"));

        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            Add(key?.GetValue("SteamPath") as string);
        }
        catch { }

        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            Add(key?.GetValue("InstallPath") as string);
        }
        catch { }

        return roots;
    }

    private static IEnumerable<string> ReadSteamLibraryPaths(string vdfPath)
    {
        if (!File.Exists(vdfPath)) yield break;

        string text;
        try { text = File.ReadAllText(vdfPath); }
        catch { yield break; }

        var matches = System.Text.RegularExpressions.Regex.Matches(
            text,
            "\\\\"path\\\\"\\s*\\\\"(?<path>[^\\\\"]+)\\\\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var path = match.Groups["path"].Value.Replace(@"\\", @"\");
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                yield return path;
        }
    }

    private static string? FindLevelRoot(string root)
    {
        if (File.Exists(Path.Combine(root, "Info.dat")) || File.Exists(Path.Combine(root, "info.dat"))) return root;
        return Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
            .FirstOrDefault(d => File.Exists(Path.Combine(d, "Info.dat")) || File.Exists(Path.Combine(d, "info.dat")));
    }

    private static bool IsBeatSaberPath(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        (File.Exists(Path.Combine(path, "Beat Saber.exe")) || Directory.Exists(Path.Combine(path, "Beat Saber_Data")));

    private static string Sanitize(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
        value = value.Trim();
        return value.Length > 80 ? value[..80] : value;
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        foreach (var dir in Directory.EnumerateDirectories(source))
            CopyDirectory(dir, Path.Combine(destination, Path.GetFileName(dir)));
    }
}
