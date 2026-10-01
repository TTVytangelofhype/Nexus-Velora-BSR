using System.ComponentModel;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage.Attributes;

namespace NexusVeloraBSR.BeatSaber
{
    internal sealed class NexusRequestPanel : INotifyPropertyChanged
    {
        private string _status = "Bridge: waiting...";
        private string _next = "No requests queued.";
        private string _queue = "Queue is empty.";

        public event PropertyChangedEventHandler? PropertyChanged;

        [UIValue("status")]
        public string Status { get => _status; private set { _status = value; Changed(); } }

        [UIValue("next-request")]
        public string NextRequest { get => _next; private set { _next = value; Changed(); } }

        [UIValue("queue-summary")]
        public string QueueSummary { get => _queue; private set { _queue = value; Changed(); } }

        public void SetDisconnected()
        {
            Status = "Bridge: OFFLINE";
        }

        public void SetQueue(string song, string key, string requester, int count)
        {
            Status = "Bridge: CONNECTED  |  Requests: OPEN";
            NextRequest = count > 0
                ? $"NEXT: {song} [{key}]\nRequested by: {requester}"
                : "No requests queued.";
            QueueSummary = count > 0 ? $"Queue: {count} request{(count == 1 ? string.Empty : "s")}" : "Queue is empty.";
        }

        private void Changed([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
