using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NexusVeloraBSR.Core.BeatSaver
{
    public sealed class BeatSaverMap
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public BeatSaverMetadata Metadata { get; set; } = new BeatSaverMetadata();
        public BeatSaverUploader Uploader { get; set; } = new BeatSaverUploader();
        public List<BeatSaverVersion> Versions { get; set; } = new List<BeatSaverVersion>();
    }

    public sealed class BeatSaverMetadata
    {
        public string SongName { get; set; } = string.Empty;
        public string SongSubName { get; set; } = string.Empty;
        public string SongAuthorName { get; set; } = string.Empty;
        public string LevelAuthorName { get; set; } = string.Empty;
        public double Bpm { get; set; }
        public int Duration { get; set; }
    }

    public sealed class BeatSaverUploader { public string Name { get; set; } = string.Empty; }

    public sealed class BeatSaverVersion
    {
        public string Hash { get; set; } = string.Empty;
        public string DownloadURL { get; set; } = string.Empty;
        public string CoverURL { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    public sealed class BeatSaverSearchResult
    {
        public List<BeatSaverMap> Docs { get; set; } = new List<BeatSaverMap>();
    }

    public sealed class ResolvedBeatSaverMap
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SongAuthor { get; set; } = string.Empty;
        public string Mapper { get; set; } = string.Empty;
        public double Bpm { get; set; }
        public int DurationSeconds { get; set; }
        public Uri? DownloadUri { get; set; }
    }

    public sealed class BeatSaverClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public BeatSaverClient(HttpClient? httpClient = null)
        {
            _http = httpClient ?? new HttpClient();
            _http.BaseAddress = new Uri("https://api.beatsaver.com/");
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("NexusVeloraBSR/0.1.0");
            _http.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<BeatSaverMap?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            using var response = await _http.GetAsync("maps/id/" + Uri.EscapeDataString(key.Trim()), cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<BeatSaverMap>(stream, _json, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<BeatSaverMap>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query)) return Array.Empty<BeatSaverMap>();
            var url = "search/text/0?q=" + Uri.EscapeDataString(query.Trim()) + "&sortOrder=Relevance";
            using var response = await _http.GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var result = await JsonSerializer.DeserializeAsync<BeatSaverSearchResult>(stream, _json, cancellationToken).ConfigureAwait(false);
            return result?.Docs ?? (IReadOnlyList<BeatSaverMap>)Array.Empty<BeatSaverMap>();
        }

        public async Task<ResolvedBeatSaverMap?> ResolveAsync(string input, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var trimmed = input.Trim();
            BeatSaverMap? map = null;

            // BeatSaver keys are short hexadecimal IDs. Try an exact lookup first.
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, "^[0-9a-fA-F]+$"))
                map = await GetByKeyAsync(trimmed, cancellationToken).ConfigureAwait(false);

            if (map == null)
            {
                var results = await SearchAsync(trimmed, cancellationToken).ConfigureAwait(false);
                if (results.Count > 0) map = results[0];
            }

            if (map == null) return null;
            return new ResolvedBeatSaverMap
            {
                Key = map.Id,
                Name = string.IsNullOrWhiteSpace(map.Metadata.SongName) ? map.Name : map.Metadata.SongName,
                SongAuthor = map.Metadata.SongAuthorName,
                Mapper = string.IsNullOrWhiteSpace(map.Metadata.LevelAuthorName) ? map.Uploader.Name : map.Metadata.LevelAuthorName,
                Bpm = map.Metadata.Bpm,
                DurationSeconds = map.Metadata.Duration,
                DownloadUri = GetLatestDownloadUri(map)
            };
        }

        public Uri? GetLatestDownloadUri(BeatSaverMap map)
        {
            if (map.Versions == null || map.Versions.Count == 0) return null;
            var raw = map.Versions[map.Versions.Count - 1].DownloadURL;
            return Uri.TryCreate(raw, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? uri : null;
        }

        public void Dispose() => _http.Dispose();
    }
}
