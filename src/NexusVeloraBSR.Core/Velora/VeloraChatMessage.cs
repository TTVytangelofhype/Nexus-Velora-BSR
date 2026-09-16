using System.Collections.Generic;

namespace NexusVeloraBSR.Core.Velora
{
    public sealed class VeloraChatMessage
    {
        public string UserName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Source { get; set; } = "velora";
        public bool IsBroadcaster { get; set; }
        public bool IsModerator { get; set; }
        public List<string> Badges { get; set; } = new List<string>();
    }
}
