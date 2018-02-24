using SkunkLab.Channels;
using System;

namespace SkunkLab.Core
{
    public delegate void ProtocolAdapterErrorHandler (object sender, ProtocolAdapterErrorEventArgs args);
    public delegate void ProtocolAdapterCloseHandler (object sender, ProtocolAdapterCloseEventArgs args);
    public abstract class ProtocolAdapter : IDisposable
    {
        public abstract IChannel Channel { get; set; }
        public abstract event ProtocolAdapterErrorHandler OnError;
        public abstract event ProtocolAdapterCloseHandler OnClose;        

        public abstract void Init();
        public abstract void Dispose();



    }
}
