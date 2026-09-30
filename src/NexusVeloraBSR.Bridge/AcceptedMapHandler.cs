using NexusVeloraBSR.Core;
using NexusVeloraBSR.Core.BeatSaver;

namespace NexusVeloraBSR.Bridge;

public sealed class AcceptedMapHandler : IAcceptedMapHandler
{
    private readonly BeatSaberMapInstaller _installer;
    private readonly GameRefreshState _refresh;
    private readonly IConfiguration _config;

    public AcceptedMapHandler(BeatSaberMapInstaller installer, GameRefreshState refresh, IConfiguration config)
    {
        _installer = installer;
        _refresh = refresh;
        _config = config;
    }

    public async Task HandleAcceptedAsync(ResolvedBeatSaverMap map, CancellationToken cancellationToken = default)
    {
        if (!bool.TryParse(_config["NexusVeloraBSR:AutoDownload"], out var enabled) || !enabled)
            return;

        await _installer.InstallAsync(map, cancellationToken);
        _refresh.MarkMapInstalled();
    }
}
