using System;
using System.Collections.Generic;
using System.Linq;
using NexusVeloraBSR.Core.Models;

namespace NexusVeloraBSR.Core.Queue
{
    public sealed class RequestQueue
    {
        private readonly object _sync = new object();
        private readonly List<SongRequest> _items = new List<SongRequest>();
        private bool _isOpen = true;

        public bool IsOpen { get { lock (_sync) return _isOpen; } }
        public int MaxRequestsPerUser { get; set; } = 2;
        public int MaxQueueSize { get; set; } = 50;
        public IReadOnlyList<SongRequest> Items { get { lock (_sync) return _items.ToList().AsReadOnly(); } }

        public void Open() { lock (_sync) _isOpen = true; }
        public void Close() { lock (_sync) _isOpen = false; }
        public void Clear() { lock (_sync) _items.Clear(); }

        public bool TryAdd(SongRequest request, out string message)
        {
            lock (_sync)
            {
                if (!_isOpen) { message = "Song requests are currently closed."; return false; }
                if (_items.Count >= MaxQueueSize) { message = "The NEXUS BSR queue is full."; return false; }
                if (_items.Any(x => string.Equals(x.BeatSaverKey, request.BeatSaverKey, StringComparison.OrdinalIgnoreCase))) { message = "That map is already in the queue."; return false; }
                if (_items.Count(x => string.Equals(x.Requester, request.Requester, StringComparison.OrdinalIgnoreCase)) >= MaxRequestsPerUser) { message = $"{request.Requester} has reached the request limit."; return false; }
                _items.Add(request);
                message = $"Added {request.SongName} [{request.BeatSaverKey}] for {request.Requester}.";
                return true;
            }
        }

        public SongRequest? RemoveLastFor(string requester)
        {
            lock (_sync)
            {
                var item = _items.LastOrDefault(x => string.Equals(x.Requester, requester, StringComparison.OrdinalIgnoreCase));
                if (item != null) _items.Remove(item);
                return item;
            }
        }

        public SongRequest? Remove(string argument)
        {
            lock (_sync)
            {
                if (int.TryParse(argument, out var position) && position >= 1 && position <= _items.Count)
                {
                    var byPosition = _items[position - 1];
                    _items.RemoveAt(position - 1);
                    return byPosition;
                }

                var byKey = _items.FirstOrDefault(x => string.Equals(x.BeatSaverKey, argument, StringComparison.OrdinalIgnoreCase));
                if (byKey != null) _items.Remove(byKey);
                return byKey;
            }
        }

        public SongRequest? Dequeue()
        {
            lock (_sync)
            {
                if (_items.Count == 0) return null;
                var item = _items[0];
                _items.RemoveAt(0);
                return item;
            }
        }
    }
}
