using Orleans;
using Piraeus.Core.Messaging;

namespace Piraeus.GrainInterfaces
{
    public interface IMetricObserver : IGrainObserver
    {
        void NotifyMetrics(CommunicationMetrics metrics);
    }
}
