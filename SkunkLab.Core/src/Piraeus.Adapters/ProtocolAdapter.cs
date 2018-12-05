using SkunkLab.Channels;
using System;

namespace Piraeus.Adapters
{
    public abstract class ProtocolAdapter : IDisposable
    {
        public abstract IChannel Channel { get; set; }

        public abstract event EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public abstract event EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public abstract event EventHandler<ChannelObserverEventArgs> OnObserve;

        public abstract void Init();
        public abstract void Dispose();
    }
}
