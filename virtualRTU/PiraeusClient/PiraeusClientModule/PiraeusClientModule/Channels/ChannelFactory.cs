using PiraeusClientModule.Channels.WebSocket;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace PiraeusClientModule.Channels
{
    public abstract class ChannelFactory
    {
        #region TCP Channels

       

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
        /// <returns></returns>
        public static IChannel Create(bool usePrefixLength, IPAddress address, int port, IPEndPoint localEP, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return TcpChannel.Create(usePrefixLength, address, port, localEP, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

       

        #endregion
        #endregion

        

       

        #region Web Socket Channels


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

    }
}
