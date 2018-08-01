using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Core.Udp
{
    public abstract class UdpChannel : IChannel
    {
        /// <summary>
        /// Create a UDP server-side connection to send/receive.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static UdpChannel Create(UdpClient client, IPEndPoint remoteEP, CancellationToken token)
        {
            return new UdpServerChannel(client, remoteEP, token);
        }

        /// <summary>
        /// Create UDP client channel.
        /// </summary>
        /// <param name="localPort"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static UdpChannel Create(int localPort, string hostname, int port, CancellationToken token)
        {
            return new UdpClientChannel(localPort, hostname, port, token);
        }

        /// <summary>
        /// Creates UDP client channel.
        /// </summary>
        /// <param name="localPort"></param>
        /// <param name="remoteEP"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static UdpChannel Create(int localPort, IPEndPoint remoteEP, CancellationToken token)
        {
            return new UdpClientChannel(localPort, remoteEP, token);
        }


        public abstract string Id { get; internal set; }

        public abstract bool RequireBlocking { get; }

        public abstract string TypeId { get;  }

        public abstract bool IsConnected { get; }

        public abstract int Port { get; internal set; }

        public abstract ChannelState State { get; internal set; }

        public abstract bool IsEncrypted { get; internal set; }

        public abstract bool IsAuthenticated { get; internal set; }

        public abstract event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public abstract event EventHandler<ChannelCloseEventArgs> OnClose;
        public abstract event EventHandler<ChannelOpenEventArgs> OnOpen;
        public abstract event EventHandler<ChannelErrorEventArgs> OnError;
        public abstract event EventHandler<ChannelStateEventArgs> OnStateChange;

     

        public abstract Task CloseAsync();

        public abstract void Dispose();

        public abstract Task OpenAsync();

        public abstract Task ReceiveAsync();

        public abstract Task SendAsync(byte[] message);
      

        public abstract Task AddMessageAsync(byte[] message);
    }
}
