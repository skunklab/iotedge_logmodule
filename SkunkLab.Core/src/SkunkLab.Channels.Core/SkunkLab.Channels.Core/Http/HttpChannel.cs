using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Http
{
    public abstract class HttpChannel : IChannel
    {

        #region Client Channels

        public static HttpChannel Create(string endpoint, string securityToken)
        {
            return new HttpClientChannel(endpoint, securityToken);
        }

        public static HttpChannel Create(string endpoint, X509Certificate2 certificate)
        {
            return new HttpClientChannel(endpoint, certificate);
        }

        /// <summary>
        /// Creates http client channel used for sending
        /// </summary>
        /// <param name="endpoint">HTTP endpoint for the request</param>
        /// <param name="resourceUriString">Piraeus resource URI to send message.</param>
        /// <param name="contentType">HTTP content type of message</param>
        /// <param name="securityToken">Symmetric key security token as string</param>
        /// <param name="indexes">Optional indexes used to filter receivers.</param>
        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType, string securityToken, string cacheKey = null, List<KeyValuePair<string,string>> indexes = null)
        {
            return new HttpClientChannel(endpoint, resourceUriString, contentType, securityToken, cacheKey, indexes);
        }

        /// <summary>
        /// Creates http client channel used for sending
        /// </summary>
        /// <param name="endpoint">HTTP endpoint for the request.</param>
        /// <param name="resourceUriString">Piraeus resource URI to send message.</param>
        /// <param name="contentType">HTTP content type of message</param>
        /// <param name="certificate">X509 certificate used to authenticate the request.</param>
        /// <param name="indexes">Optional indexes used to filter receivers.</param>
        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType, X509Certificate2 certificate, string cacheKey = null, List<KeyValuePair<string, string>> indexes = null)
        {
            return new HttpClientChannel(endpoint, resourceUriString, contentType, certificate, cacheKey, indexes);
        }

        /// <summary>
        /// Create http client channel used to listen for messages via long polling.
        /// </summary>
        /// <param name="endpoint">HTTP endpoint for the request.</param>
        /// <param name="securityToken">Symmetric key security token as string</param>
        /// <param name="observers">Observers to receive the subscribed messages.</param>
        /// <returns></returns>
        public static HttpChannel Create(string endpoint, string securityToken, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            return new HttpClientChannel(endpoint, securityToken, observers, token);
        }

        /// <summary>
        /// Creates http client channel used to listen for messages via long polling.
        /// </summary>
        /// <param name="endpoint">HTTP endpoint for the request.</param>
        /// <param name="certificate">X509 certificate used to authenticate the request.</param>
        /// <param name="observers">Observers to receive the subscribed messages.</param>
        /// <returns></returns>
        public static HttpChannel Create(string endpoint, X509Certificate2 certificate, IEnumerable<Observer> observers, CancellationToken token = default(CancellationToken))
        {
            return new HttpClientChannel(endpoint, certificate, observers, token);
        }



        #endregion


        #region Server Channels

        public static HttpChannel Create(HttpRequestMessage request)
        {
            return new HttpServerChannel(request);
        }

        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType)
        {
            return new HttpServerChannel(endpoint, resourceUriString, contentType);
        }

        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType, string securityToken)
        {
            return new HttpServerChannel(endpoint, resourceUriString, contentType, securityToken);
        }

        public static HttpChannel Create(string endpoint, string resourceUriString, string contentType, X509Certificate2 certificate)
        {
            
            return new HttpServerChannel(endpoint, resourceUriString, contentType, certificate);
        }

        #endregion
        

        public abstract bool RequireBlocking { get; }

        public abstract string TypeId { get; }
        public abstract int Port { get; internal set; }
        public abstract bool IsConnected { get;  }
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
