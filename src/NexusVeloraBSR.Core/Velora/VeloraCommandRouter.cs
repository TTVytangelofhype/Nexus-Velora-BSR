using System;
using System.Threading.Tasks;
using NexusVeloraBSR.Core.Commands;
using NexusVeloraBSR.Core.Models;
using NexusVeloraBSR.Core.Queue;
using NexusVeloraBSR.Core.BeatSaver;

namespace NexusVeloraBSR.Core.Velora
{
    public sealed class VeloraCommandRouter
    {
        private readonly RequestQueue _queue;
        private readonly BeatSaverClient _beatSaver;

        public VeloraCommandRouter(RequestQueue queue, BeatSaverClient beatSaver)
        {
            _queue = queue;
            _beatSaver = beatSaver;
        }

        public async Task<string?> HandleAsync(VeloraChatMessage chat)
        {
            var command = CommandParser.Parse(chat.Text);
            if (command.Type == CommandType.None) return null;

            switch (command.Type)
            {
                case CommandType.Request:
                    if (string.IsNullOrWhiteSpace(command.Argument)) return "Usage: !bsr <BeatSaver ID or song name>";
                    var map = await _beatSaver.ResolveAsync(command.Argument).ConfigureAwait(false);
                    if (map == null) return $"No BeatSaver map found for '{command.Argument}'.";
                    var request = new SongRequest
                    {
                        BeatSaverKey = map.Key,
                        SongName = map.Name,
                        SongAuthor = map.SongAuthor,
                        Mapper = map.Mapper,
                        Requester = chat.UserName,
                        RequestedAtUtc = DateTime.UtcNow
                    };
                    _queue.TryAdd(request, out var result);
                    return result;

                case CommandType.Queue:
                    return _queue.Items.Count == 0 ? "The NEXUS BSR queue is empty." : $"NEXUS BSR queue: {_queue.Items.Count} song(s). Next: {_queue.Items[0].SongName} [{_queue.Items[0].BeatSaverKey}]";

                case CommandType.Undo:
                    var removed = _queue.RemoveLastFor(chat.UserName);
                    return removed == null ? "You have no queued request to remove." : $"Removed {removed.SongName} from your requests.";

                case CommandType.Help:
                    return "NEXUS BSR: !bsr <BeatSaver ID/song>, !queue, !oops, !bsrhelp";
            }

            if (!chat.IsBroadcaster && !chat.IsModerator) return "That NEXUS BSR command is for moderators only.";

            switch (command.Type)
            {
                case CommandType.Open: _queue.Open(); return "NEXUS song requests are OPEN.";
                case CommandType.Close: _queue.Close(); return "NEXUS song requests are CLOSED.";
                case CommandType.Skip:
                    var skipped = _queue.Dequeue();
                    return skipped == null ? "The queue is empty." : $"Skipped {skipped.SongName}.";
                case CommandType.Clear: _queue.Clear(); return "NEXUS BSR queue cleared.";
                default: return "Moderator command recognised but is not implemented yet.";
            }
        }
    }
}
