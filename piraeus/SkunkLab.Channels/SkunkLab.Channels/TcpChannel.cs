using SkunkLab.Channels.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    public abstract class TcpChannel : IChannel
    {
        /// <summary>
        /// Creates a new TCP server channel.
        /// </summary>
        /// <param name="client">TCP client obtained from TCP listener.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(TcpClient client, CancellationToken token)
        {
            return new TcpServerChannel(client, token);
        }

        /// <summary>
        /// Creates a new TCP server channel.
        /// </summary>
        /// <param name="client">TCP client obtained from TCP listener.</param>
        /// <param name="certificate">Server certificate used for authentication.</param>
        /// <param name="clientAuth">Determines whether to authenticate the client certificate.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(TcpClient client, X509Certificate2 certificate, bool clientAuth, CancellationToken token)
        {
            return new TcpServerChannel(client, certificate, clientAuth, token);
        }


        /// <summary>
        /// Create a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint to bind for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, IPEndPoint localEP, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, localEP, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, localEP, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPAddress address, int port, CancellationToken token)
        {
            return new TcpClientChannel(address, port, token);
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPAddress address, int port, IPEndPoint localEP, CancellationToken token)
        {
            return new TcpClientChannel(address, port, localEP, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, certificate, token);
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(hostname, port, localEP, certificate, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, certificate, token);
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(remoteEndpoint, localEP, certificate, token);
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="certificate">Certificate to authenticate client.</param>
        /// <param name="token">Cancellation token.</param>
        public static TcpChannel Create(IPAddress address, int port, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(address, port, certificate, token);
        }

        /// <summary>
        /// Create new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, CancellationToken token)
        {
            return new TcpClientChannel(address, port, localEP, certificate, token);
        }


        public abstract int Port { get; internal set; }
        public abstract bool IsConnected { get; }
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
