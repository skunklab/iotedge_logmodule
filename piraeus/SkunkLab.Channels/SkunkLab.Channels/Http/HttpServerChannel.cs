using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SkunkLab.Channels.Http
{
    public class HttpServerChannel : HttpChannel
    {
        
        /// <summary>
        /// Creates HTTP server channel for a published message or long polling
        /// </summary>
        /// <param name="request">Http request from client.</param>
        public HttpServerChannel(HttpRequestMessage request)
        {
            this.request = request;
            //contentType = request.Content.Headers.ContentType.MediaType;
            Port = HttpContext.Current.Request.Url.Port;
            
            if (request.Method == HttpMethod.Get)
            {
                //long polling
                subscriptions = GetSubscriptions(request.Headers);   //subscriptions to listen             
            }
            else
            {
                //published message to resource
                resource = GetResource(request);
                if(resource == null)
                {
                    throw new InvalidOperationException("Required resource for HTTP server channel not found.");
                }

                indexes = GetIndexes(request);
            }
        }

        public HttpServerChannel(string endpoint, string resourceUriString, string contentType)
        {
            this.endpoint = endpoint;
            this.resource = resourceUriString;
            this.contentType = contentType;
        }

        public HttpServerChannel(string endpoint, string resourceUriString, string contentType, string securityToken)
        {
            this.endpoint = endpoint;
            this.resource = resourceUriString;
            this.contentType = contentType;
            this.securityToken = securityToken;
        }

        public HttpServerChannel(string endpoint, string resourceUriString, string contentType, X509Certificate2 certificate)
        {
            this.endpoint = endpoint;
            this.resource = resourceUriString;
            this.contentType = contentType;
            this.certificate = certificate;
        }

        private string endpoint;
        private string contentType;
        private X509Certificate2 certificate;
        private string securityToken;
        private HttpRequestMessage request;
        private string[] subscriptions;
        private string resource;
        private KeyValuePair<string, string>[] indexes;

        public override string Id { get; internal set; }

        public override bool IsConnected { get; }

        public override int Port { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override ChannelState State { get; internal set; }

        public override event ChannelCloseEventHandler OnClose;
        public override event ChannelErrorEventHandler OnError;
        public override event ChannelOpenEventHandler OnOpen;
        public override event ChannelReceivedEventHandler OnReceive;
        public override event ChannelStateEventHandler OnStateChange;
        public override event ChannelRetryEventHandler OnRetry;
        public override event ChannelSentEventHandler OnSent;

        public override async Task SendAsync(byte[] message)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(endpoint);
            if(!string.IsNullOrEmpty(securityToken))
            {
                request.Headers.Add("Authorize", "Bearer " + securityToken);
            }

            if(certificate != null)
            {
                request.ClientCertificates.Add(certificate);
            }

            request.ContentType = contentType;
            request.ContentLength = message.Length;
            request.Method = "POST";

            if (string.IsNullOrEmpty(resource))
            {
                request.Headers.Add(HttpChannelConstants.RESOURCE_HEADER, resource);
            }

            using (Stream stream = request.GetRequestStream())
            {
                await stream.WriteAsync(message, 0, message.Length);
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse) 
                {
                    if(response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Accepted ||
                        response.StatusCode == HttpStatusCode.NoContent)
                    {
                        OnSent?.Invoke(this, new ChannelSentEventArgs(Id, null));
                    }
                    else
                    {
                        //invalid response code
                    }
                }
            }

            
        }

        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await TaskDone.Done;
        }

        public override async Task OpenAsync()
        {
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, request));
            await TaskDone.Done;
        }

        public override async Task ReceiveAsync()
        {
            byte[] message = await request.Content.ReadAsByteArrayAsync();
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
        }

        public override async Task CloseAsync()
        {
            await TaskDone.Done;
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        private string[] GetSubscriptions(HttpRequestHeaders headers)
        {
            IEnumerable<string> subscriptions = headers.GetValues(HttpChannelConstants.SUBSCRIBE_HEADER);
            List<string> subscriptionList = new List<string>();
            foreach (string subscription in subscriptions)
            {
                if (Uri.IsWellFormedUriString(subscription, UriKind.Absolute))
                {
                    subscriptionList.Add(subscription);
                }
                else
                {
                    throw new UriFormatException(String.Format("Invalid subscription URI {0}", subscription));
                }
            }

            return subscriptionList.Count == 0 ? null : subscriptionList.ToArray();
        }

        private KeyValuePair<string,string>[] GetIndexes(HttpRequestMessage request)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            IEnumerable<string> indexes = request.Headers.GetValues(HttpChannelConstants.INDEX_HEADER);
            if(indexes == null)
            {
                indexes = (from kp in request.GetQueryNameValuePairs()
                           where kp.Key == "index"
                           select kp.Value);
            }

            if(indexes == null)
            {
                return null;
            }

            foreach (string index in indexes)
            {
                string[] parts = index.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    list.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
                }
                else
                {
                    throw new InvalidOperationException("Indexes in HTTP server channel header not understood.");
                }
            }

            return list.ToArray();
        }

        private string GetResource(HttpRequestMessage request)
        {
            string resourceUriString = null;

            IEnumerable<string> resources = (from kp in request.GetQueryNameValuePairs()
                                            where kp.Key == "rs"
                                            select kp.Value);

            if(resources != null && resources.Count() == 1)
            {
                resourceUriString = resources.ToArray()[0];
            }
            else
            {
                IEnumerable<string> headerValue = request.Headers.GetValues(HttpChannelConstants.RESOURCE_HEADER);
                if(headerValue != null)
                {
                    resourceUriString = resources.ToArray()[0];
                }
            }

            if(resourceUriString != null)
            {
                if(Uri.IsWellFormedUriString(resourceUriString, UriKind.Absolute))
                {
                    return resourceUriString;
                }
                else
                {
                    throw new UriFormatException(String.Format("Invalid resource Uri {0}", resourceUriString));
                }
            }

            return resourceUriString;
        }

    }
}
