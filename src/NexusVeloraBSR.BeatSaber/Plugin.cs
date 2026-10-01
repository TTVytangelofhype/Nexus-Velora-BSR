using System;
using System.Net.Http;
using System.Text.RegularExpressions;
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
        private string _lastQueueSignature = string.Empty;

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

        private async Task BridgeLoopAsync(CancellationToken ct)
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:24842/"), Timeout = TimeSpan.FromSeconds(3) };
            var lastGeneration = -1L;
            var bridgeConnected = false;
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var raw = await client.GetStringAsync("api/game/pending");
                    if (!bridgeConnected)
                    {
                        Log?.Info("NEXUS bridge connected.");
                        bridgeConnected = true;
                    }

                    var state = PendingRefresh.Parse(raw);
                    if (state.Pending && state.Generation != lastGeneration)
                    {
                        Log?.Info("New NEXUS map detected; refreshing SongCore.");
                        SongCore.Loader.Instance.RefreshSongs(false);
                        lastGeneration = state.Generation;
                        await client.PostAsync("api/game/refreshed/" + state.Generation, null);
                    }

                    await UpdateQueueStatusAsync(client);
                }
                catch (Exception ex)
                {
                    if (bridgeConnected)
                    {
                        Log?.Warn("NEXUS bridge disconnected; waiting for it to return. " + ex.Message);
                        bridgeConnected = false;
                    }
                }
                try { await Task.Delay(1500, ct); } catch (TaskCanceledException) { }
            }
        }

        private async Task UpdateQueueStatusAsync(HttpClient client)
        {
            var json = await client.GetStringAsync("api/queue");
            var first = Regex.Match(json ?? string.Empty,
                "\\{[^{}]*?\\\"beatSaverKey\\\"\\s*:\\s*\\\"(?<key>[^\\\"]+)\\\"[^{}]*?\\\"songName\\\"\\s*:\\s*\\\"(?<song>[^\\\"]+)\\\"[^{}]*?\\\"requester\\\"\\s*:\\s*\\\"(?<requester>[^\\\"]+)\\\"",
                RegexOptions.IgnoreCase);

            if (!first.Success)
            {
                if (_lastQueueSignature != "empty")
                {
                    Log?.Info("NEXUS request queue is empty.");
                    _lastQueueSignature = "empty";
                }
                return;
            }

            var key = first.Groups["key"].Value;
            var song = first.Groups["song"].Value;
            var requester = first.Groups["requester"].Value;
            var signature = key + "|" + requester;
            if (signature == _lastQueueSignature) return;

            var count = Regex.Matches(json ?? string.Empty, "\\\"beatSaverKey\\\"\\s*:", RegexOptions.IgnoreCase).Count;
            Log?.Info($"NEXUS NEXT REQUEST: {song} [{key}] requested by {requester}. Queue: {count}.");
            _lastQueueSignature = signature;
        }
    }
}
