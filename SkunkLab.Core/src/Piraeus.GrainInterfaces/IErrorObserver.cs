using System;
using Orleans;

namespace Piraeus.GrainInterfaces
{
    public interface IErrorObserver : IGrainObserver
    {
        void NotifyError(string grainId, Exception error);
    }
}
