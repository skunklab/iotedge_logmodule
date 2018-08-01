using SkunkLab.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Channels
{
    public abstract class CommunicationAdapter : IDisposable
    {
        public abstract IChannel Channel { get; set; }

        public abstract event System.EventHandler<ComAdapterEventArgs> OnError;
        public abstract event System.EventHandler<ComAdapterEventArgs> OnClose;

        public abstract void Init();
        public abstract void Dispose();
    }
}
