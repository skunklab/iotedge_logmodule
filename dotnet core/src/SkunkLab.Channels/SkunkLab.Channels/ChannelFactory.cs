using SkunkLab.Channels.Tcp;
using SkunkLab.Channels.Udp;
using SkunkLab.Channels.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using SkunkLab.Channels.Http;

namespace SkunkLab.Channels
{
    public abstract class ChannelFactory
    {
        #region TCP Channels

        #region TCP Server Channels


        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, TcpClient client, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, client, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="certificate"></param>
        /// <param name="clientAuth"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, TcpClient client, X509Certificate2 certificate, bool clientAuth, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, client, certificate, clientAuth, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP server channel
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, TcpClient client, Dictionary<string, byte[]> presharedKeys, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, client, presharedKeys, blockSize, maxBufferSize, token);
        }

        #endregion

        #region TCP Client Channels
        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, string hostname, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, hostname, port, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, string hostname, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, hostname, port, localEP, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, remoteEndpoint, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, remoteEndpoint, localEP, blockSize, maxBufferSize, token);
        }



        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPAddress address, int port, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, address, port, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, address, port, localEP, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, string hostname, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, hostname, port, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, string hostname, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, hostname, port, localEP, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, remoteEndpoint, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPEndPoint remoteEndpoint, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, remoteEndpoint, localEP, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPAddress address, int port, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, address, port, certificate, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, X509Certificate2 certificate, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, address, port, localEP, certificate, blockSize, maxBufferSize, token);
        }


        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="localEP"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, address, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPAddress address, int port, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, address, port, pskIdentity, psk, blockSize, maxBufferSize, token);
        }


        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="localEP"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, string hostname, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, hostname, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, string hostname, int port, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, hostname, port, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="remoteEP"></param>
        /// <param name="localEP"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPEndPoint remoteEP, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, remoteEP, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        /// <summary>
        /// Creates TCP client channel
        /// </summary>
        /// <param name="usePrefixLength"></param>
        /// <param name="remoteEP"></param>
        /// <param name="pskIdentity"></param>
        /// <param name="psk"></param>
        /// <param name="blockSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPEndPoint remoteEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, remoteEP, pskIdentity, psk, blockSize, maxBufferSize, token);
        }







        #endregion
        #endregion

        #region HTTP Channels

        #region HTTP Server Channels

        /// <summary>
        /// HTTP server channel used to receive messages
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IChannel Create(HttpRequestMessage request)
        {
            return HttpChannel.Create(request);
        }

        /// <summary>
        /// HTTP server channel used to transmit messages to endpoints.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, string resourceUriString, string contentType)
        {
            return HttpChannel.Create(endpoint, resourceUriString, contentType);
        }

        /// <summary>
        /// HTTP server channel used to transmit messages to endpoints.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="securityToken"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, string resourceUriString, string contentType, string securityToken)
        {
            return HttpChannel.Create(endpoint, resourceUriString, contentType, securityToken);
        }

        #endregion

        #region HTTP Client Channels


        /// <summary>
        /// Creates HTTP send-only channel
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="securityToken"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, string securityToken)
        {
            return HttpChannel.Create(endpoint, securityToken);
        }

        /// <summary>
        /// Creates HTTP send-only channel
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="certficate"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, X509Certificate2 certficate)
        {
            return HttpChannel.Create(endpoint, certficate);
        }

        /// <summary>
        /// Creates HTTP receive only channel (long polling)
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="securityToken"></param>
        /// <param name="observers"></param>
        /// <param name="token"></param>
        /// <returns></returns>


        public static IChannel Create(string endpoint, string securityToken, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            return HttpChannel.Create(endpoint, securityToken, observers, token);
        }

        /// <summary>
        /// Creates HTTP receive only channel (long polling)
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="certificate"></param>
        /// <param name="observers"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(string endpoint, X509Certificate2 certificate, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            return HttpChannel.Create(endpoint, certificate, observers, token);
        }

        #endregion

        #endregion

        #region Web Socket Channels

        #region Web Socket Server Channels

        /// <summary>
        /// Create Web socket server channel.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(HttpRequestMessage request, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(request, config, token);
        }

        #endregion

        #region Web Socket Client Channels

        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, config, token);
        }

        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="subProtocol"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, subProtocol, config, token);
        }

        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="securityToken"></param>
        /// <param name="subProtocol"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, string securityToken, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, securityToken, subProtocol, config, token);
        }


        /// <summary>
        /// Creates Web socket client channel
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="certificate"></param>
        /// <param name="subProtocol"></param>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(Uri endpointUri, X509Certificate2 certificate, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            return WebSocketChannel.Create(endpointUri, certificate, subProtocol, config, token);
        }
        #endregion
        #endregion

        #region UDP Channels

        #region UDP Server Channels

        /// <summary>
        /// Creates UDP server channel
        /// </summary>
        /// <param name="localAddress"></param>
        /// <param name="localPort"></param>
        /// <param name="remoteAddress"></param>
        /// <param name="remotePort"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(UdpClient client, IPEndPoint remoteEP, CancellationToken token)
        {
            return UdpChannel.Create(client, remoteEP, token);
        }

        #endregion

        #region UDP Client Channels

        /// <summary>
        /// Creates UDP client channel
        /// </summary>
        /// <param name="localEP"></param>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(int localPort, string hostname, int port, CancellationToken token)
        {
            return UdpChannel.Create(localPort, hostname, port, token);
        }

        /// <summary>
        /// Creates UDP client channel
        /// </summary>
        /// <param name="localEP"></param>
        /// <param name="remoteAddress"></param>
        /// <param name="port"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IChannel Create(int localPort, IPEndPoint remoteEP, CancellationToken token)
        {
            return UdpChannel.Create(localPort, remoteEP, token);
        }

        #endregion

        #endregion

    }
}
