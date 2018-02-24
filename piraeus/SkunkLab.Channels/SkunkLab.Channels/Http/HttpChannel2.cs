//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SkunkLab.Channels.Http
//{
//    public class HttpChannel2 : IChannel
//    {
//        public HttpChannel2(Uri endpointUri, CancellationToken token)
//        {
//            endpoint = endpointUri;
//            this.token = token;
//        }

//        public HttpChannel2(Uri endpointUri, string securityToken, CancellationToken token)
//        {
//            endpoint = endpointUri;
//            this.securityToken = securityToken;
//            this.token = token;
//        }

//        public HttpChannel2(Uri endpointUri, X509Certificate2 certificate, CancellationToken token)
//        {
//            endpoint = endpointUri;
//            this.certificate = certificate;
//            this.token = token;
//        }
//        private Uri endpoint;
//        private string securityToken;
//        private X509Certificate2 certificate;
//        private ChannelState _state;
//        private CancellationToken token;
//        public bool IsConnected { get; internal set; }

//        public string Id { get; set; }
//        public int Port { get; set; }
//        public ChannelState State
//        {
//            get { return _state; }
//            set
//            {
//                if(value != _state)
//                {
//                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
//                }

//                _state = value;
//            }
//        }

//        public event ChannelReceivedEventHandler OnReceive;
//        public event ChannelCloseEventHandler OnClose;
//        public event ChannelOpenEventHandler OnOpen;
//        public event ChannelErrorEventHandler OnError;
//        public event ChannelStateEventHandler OnStateChange;

//        public async Task AddMessageAsync(byte[] message)
//        {
//            await TaskDone.Done;
//        }

//        public async Task CloseAsync()
//        {
//            await TaskDone.Done;
//        }

//        public void Dispose()
//        {
            
//        }

//        public async Task OpenAsync()
//        {
//            await TaskDone.Done;
//        }

//        public async Task ReceiveAsync()
//        {
//            while(!token.IsCancellationRequested)
//            {
//                try
//                {
                    
//                    using (var client = new HttpClient())
//                    {
//                        client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

//                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
//                        if (!string.IsNullOrEmpty(securityToken))
//                        {
//                            request.Headers.Add("Authorization", securityToken);
//                        }

//                        using (HttpResponseMessage response = await client.SendAsync(
//                            request,
//                            HttpCompletionOption.ResponseHeadersRead))
//                        {
//                            if (response.StatusCode == HttpStatusCode.Accepted ||
//                                response.StatusCode == HttpStatusCode.OK ||
//                                response.StatusCode == HttpStatusCode.NoContent)
//                            {

//                                byte[] message = await response.Content.ReadAsByteArrayAsync();

//                                OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
//                            }
//                            else
//                            {
//                                throw new Exception("Invalid status code.");
//                            }
//                        }
//                    }
//                }
//                catch(WebException we)
//                {
//                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, we));
//                }
//                catch(AggregateException ae)
//                {
//                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae.Flatten()));
//                }
//                catch(Exception ex)
//                {
//                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));

//                }
//            }
//        }

//        public async Task SendAsync(byte[] message)
//        {
//            using (HttpClient client = new HttpClient())
//            {
//                client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

//                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
//                if (!string.IsNullOrEmpty(securityToken))
//                {
//                    request.Headers.Add("Authorization", securityToken);
//                }

//                if(certificate != null)
//                {
                    
//                }

//                using (HttpResponseMessage response = await client.SendAsync(
//                    request,
//                    HttpCompletionOption.ResponseHeadersRead))
//                {
//                    if (response.StatusCode == HttpStatusCode.Accepted ||
//                        response.StatusCode == HttpStatusCode.OK ||
//                        response.StatusCode == HttpStatusCode.NoContent)
//                    {

//                        //byte[] message = await response.Content.ReadAsByteArrayAsync();

//                        //OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
//                    }
//                    else
//                    {
//                        throw new Exception("Invalid status code.");
//                    }
//                }
//            }
//        }
//    }
//}
