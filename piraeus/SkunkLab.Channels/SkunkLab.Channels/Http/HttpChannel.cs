using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Http
{
    public abstract class HttpChannel : IChannel
    {
        public abstract int Port { get; internal set; }
        public abstract bool IsConnected { get;  }
        public abstract string Id { get; internal set; }

        public abstract bool IsEncrypted { get; internal set; }

        public abstract bool IsAuthenticated { get; internal set; }

        public abstract ChannelState State { get; internal set; }

        public abstract event ChannelReceivedEventHandler OnReceive;
        public abstract event ChannelCloseEventHandler OnClose;
        public abstract event ChannelOpenEventHandler OnOpen;
        public abstract event ChannelErrorEventHandler OnError;
        public abstract event ChannelStateEventHandler OnStateChange;
        public abstract event ChannelRetryEventHandler OnRetry;
        public abstract event ChannelSentEventHandler OnSent;

        public abstract Task CloseAsync();

        public abstract void Dispose();

        public abstract Task OpenAsync();

        public abstract Task ReceiveAsync();

        public abstract Task SendAsync(byte[] message);

        public abstract Task AddMessageAsync(byte[] message);
    }
}
