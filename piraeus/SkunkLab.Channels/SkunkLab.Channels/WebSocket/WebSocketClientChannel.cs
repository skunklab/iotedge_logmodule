using System;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.WebSocket
{
    public class WebSocketClientChannel : WebSocketChannel
    {
        #region ctor
        public WebSocketClientChannel(Uri endpointUri, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.config = config;
            this.token = token;            
        }

        public WebSocketClientChannel(Uri endpointUri, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.subProtocol = subProtocol;
            this.config = config;
            this.token = token;
        }

        public WebSocketClientChannel(Uri endpointUri, string securityToken, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.securityToken = securityToken;
            this.subProtocol = subProtocol;
            this.config = config;
            this.token = token;
        }

        public WebSocketClientChannel(Uri endpointUri, X509Certificate2 certificate, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.certificate = certificate;
            this.subProtocol = subProtocol;
            this.config = config;
            this.token = token;
        }

        #endregion

        private Uri endpoint;
        private CancellationToken token;
        private ClientWebSocket client;
        private WebSocketConfig config;
        private string subProtocol;
        private string securityToken;
        private X509Certificate2 certificate;
        private ChannelState _state;
        private bool disposed;

        public override event ChannelCloseEventHandler OnClose;
        public override event ChannelErrorEventHandler OnError;
        public override event ChannelOpenEventHandler OnOpen;
        public override event ChannelReceivedEventHandler OnReceive;
        public override event ChannelStateEventHandler OnStateChange;
        public override event ChannelRetryEventHandler OnRetry;
        public override event ChannelSentEventHandler OnSent;

        public override string Id { get;  internal set; }

        public override bool IsConnected
        {
            get { return State == ChannelState.Open; }
        }

        public override int Port { get;  internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override ChannelState State
        {
            get { return _state; }
            internal set
            {
                if(value != _state)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }
                _state = value;
            }
        }

        public override void Open()
        {
            if (client != null)
            {
                Task task =  client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal", token);
                Task.WaitAll(task);
                client.Dispose();
                
            }


            client = new ClientWebSocket();            
            

            if (!string.IsNullOrEmpty(subProtocol))
            {
                client.Options.AddSubProtocol(subProtocol);
            }

            if (!string.IsNullOrEmpty(securityToken))
            {
                client.Options.SetRequestHeader("Authorize", "Bearer " + securityToken);
            }

            if (certificate != null)
            {
                client.Options.ClientCertificates.Add(certificate);
            }

            State = ChannelState.Connecting;

            try
            {
                Task task =  client.ConnectAsync(endpoint, token);
                Task.WaitAll(task);
                State = ChannelState.Open;
                IsAuthenticated = true;
               
                OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
            }
            catch (AggregateException ae)
            {
                State = ChannelState.Aborted;
                throw ae.Flatten();
            }
            catch (Exception ex)
            {
                State = ChannelState.Aborted;
                throw ex;
            }
        }

        public override void Send(byte[] message)
        {
            Task task = SendAsync(message);
            Task.WaitAll(task);
            OnSent?.Invoke(Id, null);
        }

        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await TaskDone.Done;

        }

        public override async Task OpenAsync()
        {
            if(client != null)
            {
                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal", token);
                client.Dispose();
            }

            client = new ClientWebSocket();

            if(!string.IsNullOrEmpty(subProtocol))
            {
                client.Options.AddSubProtocol(subProtocol);
            }

            if(!string.IsNullOrEmpty(securityToken))
            {
                client.Options.SetRequestHeader("Authorize", securityToken);
            }

            if(certificate != null)
            {
                client.Options.ClientCertificates.Add(certificate);
            }

            State = ChannelState.Connecting;
            try
            {
                await client.ConnectAsync(endpoint, token);
                State = ChannelState.Open;
                OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
            }
            catch(AggregateException ae)
            {
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ae.Flatten()));
                State = ChannelState.Aborted;
            }
            catch(Exception ex)
            {
                State = ChannelState.Aborted;
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }            
        }

        public override async Task ReceiveAsync()
        {
            while (!token.IsCancellationRequested && IsConnected)
            {
                ChannelReceivedEventArgs args = null;
                WebSocketMessage message = await WebSocketMessageReader.ReadMessageAsync(client, new byte[config.ReceiveLoopBufferSize], config.MaxIncomingMessageSize, token);
                if(message.Data != null)
                {
                    if(message.MessageType == WebSocketMessageType.Binary)
                    {
                        args = new ChannelReceivedEventArgs(Id, message.Data as byte[]);
                    }
                    else if(message.MessageType == WebSocketMessageType.Text)
                    {
                        args = new ChannelReceivedEventArgs(Id, Encoding.UTF8.GetBytes(message.Data as string));
                    }
                    else
                    {
                        State = ChannelState.ClosedReceived;
                        break;
                    }

                    OnReceive?.Invoke(this, args);
                }
            }

            await CloseAsync();
        }

        public override async Task SendAsync(byte[] message)
        {
            if(message.Length > config.MaxIncomingMessageSize)
            {
                throw new InvalidOperationException("Exceeds max message size.");
            }

            if (message.Length <= config.SendBufferSize)
            {
                await client.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, token);
            }
            else
            {
                int offset = 0;
                byte[] buffer = null;
                while(message.Length - offset > config.SendBufferSize)
                {
                    buffer = new byte[config.SendBufferSize];
                    Buffer.BlockCopy(message, offset, buffer, 0, buffer.Length);
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, token);
                    offset += buffer.Length;        
                }

                buffer = new byte[message.Length - offset];
                await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, token);
            }
        }

        public override async Task CloseAsync()
        {
            if (client != null && client.State == WebSocketState.Open)
            {
                State = ChannelState.ClosedReceived;
                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal", token);
            }

            if (State != ChannelState.Closed)
            {
                State = ChannelState.Closed;

                OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));
            }
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposed)
            {                
                disposed = true;

                if(client != null)
                {
                    if(client.State == WebSocketState.Open)
                    {
                        Task task = client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal Disposed", token);
                        Task.WhenAll(task);                      
                    }

                    client.Dispose();
                }
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }
    }
}
