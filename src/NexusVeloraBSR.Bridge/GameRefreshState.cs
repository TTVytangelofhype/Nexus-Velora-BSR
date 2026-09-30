namespace NexusVeloraBSR.Bridge;

public sealed class GameRefreshState
{
    private long _generation;
    private long _acknowledged;

    public long Generation => Interlocked.Read(ref _generation);
    public bool Pending => Generation > Interlocked.Read(ref _acknowledged);

    public long MarkMapInstalled() => Interlocked.Increment(ref _generation);

    public void Acknowledge(long generation)
    {
        var current = Interlocked.Read(ref _acknowledged);
        while (generation > current)
        {
            var observed = Interlocked.CompareExchange(ref _acknowledged, generation, current);
            if (observed == current) return;
            current = observed;
        }
    }
}
