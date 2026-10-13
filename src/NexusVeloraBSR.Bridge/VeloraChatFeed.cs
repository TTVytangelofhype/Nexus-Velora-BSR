namespace NexusVeloraBSR.Bridge;

// Only messages received after the bridge connects are retained. No disk history.
public sealed class VeloraChatFeed
{
    private readonly object _sync = new();
    private readonly Queue<ChatLine> _lines = new();
    private long _sequence;
    public sealed record ChatLine(long Id, string User, string Text);

    public void Add(string user, string text)
    {
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(text)) return;
        lock (_sync)
        {
            _lines.Enqueue(new ChatLine(++_sequence, user.Length > 40 ? user[..40] : user,
                text.Length > 300 ? text[..300] : text));
            while (_lines.Count > 30) _lines.Dequeue();
        }
    }

    public ChatLine[] Snapshot()
    {
        lock (_sync) return _lines.ToArray();
    }
}
