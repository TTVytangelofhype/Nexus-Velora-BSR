using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IPA;
using IPALogger = IPA.Logging.Logger;
using UnityEngine.SceneManagement;

namespace NexusVeloraBSR.BeatSaber
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public sealed class Plugin
    {
        internal static IPALogger? Log { get; private set; }
        private CancellationTokenSource? _cts;
        private Task? _worker;
        private string _lastQueueSignature = string.Empty;
        private readonly NativeRequestDisplay _display = new NativeRequestDisplay();
        private SynchronizationContext? _unityContext;

        [Init]
        public void Init(IPALogger logger) { Log = logger; logger.Info("NEXUS Velora BSR adapter initialized."); }

        [OnEnable]
        public void OnEnable()
        {
            _unityContext = SynchronizationContext.Current;
            _display.EnsureCreated();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            UpdateOverlayForScene(SceneManager.GetActiveScene());
            _cts = new CancellationTokenSource();
            _worker = Task.Run(() => BridgeLoopAsync(_cts.Token));
        }

        [OnDisable]
        public void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _cts?.Cancel();
            try { _worker?.Wait(1500); } catch { }
            _cts?.Dispose(); _cts = null; _worker = null;
        }

        private void OnActiveSceneChanged(Scene previous, Scene current)
        {
            UpdateOverlayForScene(current);
        }

        private void UpdateOverlayForScene(Scene scene)
        {
            var name = scene.name ?? string.Empty;
            // Beat Saber menu scenes contain MenuCore. Hide only after entering an actual
            // gameplay scene; do not treat persistent Core/GameCore bootstrap scenes as gameplay.
            var menu = name.IndexOf("MenuCore", StringComparison.OrdinalIgnoreCase) >= 0;
            var gameplay = !menu &&
                (name.Equals("StandardGameplay", StringComparison.OrdinalIgnoreCase) ||
                 name.Equals("MultiplayerGameplay", StringComparison.OrdinalIgnoreCase) ||
                 name.IndexOf("GameplayCore", StringComparison.OrdinalIgnoreCase) >= 0);
            _display.SetVisible(!gameplay);
            Log?.Info($"NEXUS overlay scene='{name}' visible={!gameplay}.");
        }

        private void RunOnUnityThread(Action action)
        {
            var context = _unityContext;
            if (context != null) context.Post(_ => action(), null);
            else action();
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
                    RunOnUnityThread(() => _display.SetDisconnected());
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
                RunOnUnityThread(() => _display.SetConnected(string.Empty, string.Empty, string.Empty, 0));
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
            RunOnUnityThread(() => _display.SetConnected(song, key, requester, count));
            Log?.Info($"NEXUS NEXT REQUEST: {song} [{key}] requested by {requester}. Queue: {count}.");
            _lastQueueSignature = signature;
        }
    }
}
