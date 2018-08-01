using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Core.Tcp
{
    public abstract class TcpChannel : IChannel
    {
        /// <summary>
        /// Creates a new TCP server channel.
        /// </summary>
        /// <param name="client">TCP client obtained from TCP listener.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, TcpClient client,  int blockSize, int maxBufferSize, CancellationToken token)
        {
            if(usePrefixLength)
            {
                return new TcpServerChannel(client, maxBufferSize, token);
            }
            else
            {
                return new TcpServerChannel2(client, blockSize, maxBufferSize, token);
            }
        }

        public static TcpChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(address, port, localEP, pskIdentity, psk, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(address, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates a new TCP server channel.
        /// </summary>
        /// <param name="client">TCP client obtained from TCP listener.</param>
        /// <param name="certificate">Server certificate used for authentication.</param>
        /// <param name="clientAuth">Determines whether to authenticate the client certificate.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, TcpClient client,  X509Certificate2 certificate, bool clientAuth, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpServerChannel(client, certificate, clientAuth, maxBufferSize, token);
            }
            else
            {
                return new TcpServerChannel2(client, certificate, clientAuth, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Create new TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, TcpClient client, Dictionary<string,byte[]> presharedKeys, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpServerChannel(client, presharedKeys, maxBufferSize, token);
            }
            else
            {
                return new TcpServerChannel2(client, presharedKeys, blockSize, maxBufferSize, token);
            }
        }

        

        /// <summary>
        /// Create a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, string hostname, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(hostname, port, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(hostname, port, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint to bind for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, string hostname, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(hostname, port, localEP, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(hostname, port, localEP, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(remoteEndpoint, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(remoteEndpoint, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(remoteEndpoint, localEP, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(remoteEndpoint, localEP, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPAddress address, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(address, port, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(address, port, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(address, port, localEP, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(address, port, localEP, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="hostname">Host name of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, string hostname, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(hostname, port, certificate, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(hostname, port, certificate, blockSize, maxBufferSize, token);
            }
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
        public static TcpChannel Create(bool usePrefixLength, string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(hostname, port, localEP, certificate, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(hostname, port, localEP, certificate, blockSize, maxBufferSize, token);
            }
        }

        

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(remoteEndpoint, certificate, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(remoteEndpoint, certificate, blockSize, maxBufferSize, token);
            }
        }

        /// <summary>
        /// Creates new TCP client channel.
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of server to connect.</param>
        /// <param name="localEP">Local endpoint for client connection.</param>
        /// <param name="certificate">Certificate used to authenticate the client.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public static TcpChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBuferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(remoteEndpoint, localEP, certificate, maxBuferSize, token);
            }
            else
            {
                return new TcpClientChannel2(remoteEndpoint, localEP, certificate, blockSize, maxBuferSize, token);
            }
        }

        /// <summary>
        /// Creates a new TCP client channel.
        /// </summary>
        /// <param name="address">Address of server to connect.</param>
        /// <param name="port">Port of server to connect.</param>
        /// <param name="certificate">Certificate to authenticate client.</param>
        /// <param name="token">Cancellation token.</param>
        public static TcpChannel Create(bool usePrefixLength, IPAddress address, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(address, port, certificate, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(address, port, certificate, blockSize, maxBufferSize, token);
            }
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
        public static TcpChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            if (usePrefixLength)
            {
                return new TcpClientChannel(address, port, localEP, certificate, maxBufferSize, token);
            }
            else
            {
                return new TcpClientChannel2(address, port, localEP, certificate, blockSize, maxBufferSize, token);
            }
        }

       

        public abstract bool RequireBlocking { get; }
        public abstract int Port { get; internal set; }
        public abstract string TypeId { get; }
        public abstract bool IsConnected { get; }
        public abstract string Id { get; internal set; }

        public abstract bool IsEncrypted { get; internal set; }

        public abstract bool IsAuthenticated { get; internal set; }

        public abstract ChannelState State { get; internal set; }

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
