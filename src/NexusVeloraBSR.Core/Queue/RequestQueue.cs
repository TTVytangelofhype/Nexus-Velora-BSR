using System;
using System.Collections.Generic;
using System.Linq;
using NexusVeloraBSR.Core.Models;

namespace NexusVeloraBSR.Core.Queue
{
    public sealed class RequestQueue
    {
        private readonly List<SongRequest> _items = new List<SongRequest>();
        public bool IsOpen { get; private set; } = true;
        public int MaxRequestsPerUser { get; set; } = 2;
        public IReadOnlyList<SongRequest> Items => _items.AsReadOnly();

        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;
        public void Clear() => _items.Clear();

        public bool TryAdd(SongRequest request, out string message)
        {
            if (!IsOpen) { message = "Song requests are currently closed."; return false; }
            if (_items.Any(x => string.Equals(x.BeatSaverKey, request.BeatSaverKey, StringComparison.OrdinalIgnoreCase))) { message = "That map is already in the queue."; return false; }
            if (_items.Count(x => string.Equals(x.Requester, request.Requester, StringComparison.OrdinalIgnoreCase)) >= MaxRequestsPerUser) { message = $"{request.Requester} has reached the request limit."; return false; }
            _items.Add(request);
            message = $"Added {request.SongName} [{request.BeatSaverKey}] for {request.Requester}.";
            return true;
        }

        public SongRequest? RemoveLastFor(string requester)
        {
            var item = _items.LastOrDefault(x => string.Equals(x.Requester, requester, StringComparison.OrdinalIgnoreCase));
            if (item != null) _items.Remove(item);
            return item;
        }

        public SongRequest? Dequeue()
        {
            if (_items.Count == 0) return null;
            var item = _items[0];
            _items.RemoveAt(0);
            return item;
        }
    }
}
