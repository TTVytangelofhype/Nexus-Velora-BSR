using System.Text.RegularExpressions;
namespace NexusVeloraBSR.BeatSaber
{
    internal sealed class PendingRefresh
    {
        public bool Pending { get; private set; }
        public long Generation { get; private set; }
        public static PendingRefresh Parse(string json)
        {
            var pending = Regex.IsMatch(json ?? "", "\\\"pending\\\"\\s*:\\s*true", RegexOptions.IgnoreCase);
            var match = Regex.Match(json ?? "", "\\\"generation\\\"\\s*:\\s*(\\d+)", RegexOptions.IgnoreCase);
            long n = 0; if (match.Success) long.TryParse(match.Groups[1].Value, out n);
            return new PendingRefresh { Pending = pending, Generation = n };
        }
    }
}
