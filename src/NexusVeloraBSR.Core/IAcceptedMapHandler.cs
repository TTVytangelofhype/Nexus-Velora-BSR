using System.Threading;
using System.Threading.Tasks;
using NexusVeloraBSR.Core.BeatSaver;

namespace NexusVeloraBSR.Core
{
    public interface IAcceptedMapHandler
    {
        Task HandleAcceptedAsync(ResolvedBeatSaverMap map, CancellationToken cancellationToken = default);
    }
}
