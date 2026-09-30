using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IPA;
using IPALogger = IPA.Logging.Logger;

namespace NexusVeloraBSR.BeatSaber
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public sealed class Plugin
    {
        internal static IPALogger? Log { get; private set; }
        private CancellationTokenSource? _cts;
        private Task? _worker;

        [Init]
        public void Init(IPALogger logger) { Log = logger; logger.Info("NEXUS Velora BSR adapter initialized."); }

        [OnEnable]
        public void OnEnable()
        {
            _cts = new CancellationTokenSource();
            _worker = Task.Run(() => BridgeLoopAsync(_cts.Token));
        }

        [OnDisable]
        public void OnDisable()
        {
            _cts?.Cancel();
            try { _worker?.Wait(1500); } catch { }
            _cts?.Dispose(); _cts = null; _worker = null;
        }

        private static async Task BridgeLoopAsync(CancellationToken ct)
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:24842/"), Timeout = TimeSpan.FromSeconds(3) };
            var lastGeneration = -1L;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var raw = await client.GetStringAsync("api/game/pending");
                    var state = PendingRefresh.Parse(raw);
                    if (state.Pending && state.Generation != lastGeneration)
                    {
                        Log?.Info("New NEXUS map detected; refreshing SongCore.");
                        SongCore.Loader.Instance.RefreshSongs(false);
                        lastGeneration = state.Generation;
                        await client.PostAsync("api/game/refreshed/" + state.Generation, null);
                    }
                }
                catch (Exception ex) { Log?.Debug("NEXUS bridge unavailable: " + ex.Message); }
                try { await Task.Delay(1500, ct); } catch (TaskCanceledException) { }
            }
        }
    }
}
