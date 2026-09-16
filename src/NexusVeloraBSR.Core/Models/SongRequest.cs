using System;

namespace NexusVeloraBSR.Core.Models
{
    public sealed class SongRequest
    {
        public string BeatSaverKey { get; set; } = string.Empty;
        public string SongName { get; set; } = string.Empty;
        public string SongAuthor { get; set; } = string.Empty;
        public string Mapper { get; set; } = string.Empty;
        public string Requester { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
