using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PiraeusClientModule.Channels.WebSocket
{
    public class WebSocketClientChannel : WebSocketChannel
    {
        #region ctor
        public WebSocketClientChannel(Uri endpointUri, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.config = config;
            this.token = token;
            Id = "ws-" + Guid.NewGuid().ToString();
            sendQueue = new TaskQueue();
            queue = new ConcurrentQueue<byte[]>();
        }

        public WebSocketClientChannel(Uri endpointUri, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.subProtocol = subProtocol;
            this.config = config;
            this.token = token;
            Id = "ws-" + Guid.NewGuid().ToString();
            sendQueue = new TaskQueue();
            queue = new ConcurrentQueue<byte[]>();
        }

        public WebSocketClientChannel(Uri endpointUri, string securityToken, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.securityToken = securityToken;
            this.subProtocol = subProtocol;
            this.config = config;
            this.token = token;
            Id = "ws-" + Guid.NewGuid().ToString();
            sendQueue = new TaskQueue();
            queue = new ConcurrentQueue<byte[]>();
        }

        public WebSocketClientChannel(Uri endpointUri, X509Certificate2 certificate, string subProtocol, WebSocketConfig config, CancellationToken token)
        {
            endpoint = endpointUri;
            this.certificate = certificate;
            this.subProtocol = subProtocol;
            this.config = config;
            this.token = token;
            Id = "ws-" + Guid.NewGuid().ToString();
            sendQueue = new TaskQueue();
            queue = new ConcurrentQueue<byte[]>();

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
        private TaskQueue sendQueue;
        private ConcurrentQueue<byte[]> queue;


        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;
        

        
        public override string Id { get;  internal set; }

        public override string TypeId { get { return "WebSocket"; } }

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
                sendQueue.Enqueue(() => client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal", token));

                //Task task =  client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal", token);
                //Task.WaitAll(task);
                client.Dispose();
                
            }


            client = new ClientWebSocket();            
            

            if (!string.IsNullOrEmpty(subProtocol))
            {
                client.Options.AddSubProtocol(subProtocol);
            }

            if (!string.IsNullOrEmpty(securityToken))
            {
                client.Options.SetRequestHeader("Authorization", String.Format("Bearer {0}", securityToken));
            }

            if (certificate != null)
            {
                client.Options.ClientCertificates.Add(certificate);
            }

            State = ChannelState.Connecting;

            try
            {
                //Task task =  client.ConnectAsync(endpoint, token);
                //Task.WaitAll(task);

                sendQueue.Enqueue(() => client.ConnectAsync(endpoint, token));
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
            
        }

        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await Task.CompletedTask;

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
                client.Options.SetRequestHeader("Authorization", String.Format("Bearer {0}",securityToken));
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
            try
            {
                while (!token.IsCancellationRequested && IsConnected)
                {
                    ChannelReceivedEventArgs args = null;

                    WebSocketMessage message = await WebSocketMessageReader.ReadMessageAsync(client, new byte[config.ReceiveLoopBufferSize], config.MaxIncomingMessageSize, token);
                    if (message.Data != null)
                    {
                        if (message.MessageType == WebSocketMessageType.Binary)
                        {
                            args = new ChannelReceivedEventArgs(Id, message.Data as byte[]);
                        }
                        else if (message.MessageType == WebSocketMessageType.Text)
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
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Web socket receive fail");
                Trace.TraceError(ex.Message);
            }

            await CloseAsync();
        }

        public override async Task SendAsync(byte[] message)
        {
            if (message.Length > config.MaxIncomingMessageSize)
            {
                throw new InvalidOperationException("Exceeds max message size.");
            }

            queue.Enqueue(message);
            

            try
            {
                while (!queue.IsEmpty)
                {
                    byte[] msg = null;
                    bool isDequeued = queue.TryDequeue(out msg);
                    if (isDequeued)
                    {
                        if (msg.Length <= config.SendBufferSize)
                        {
                            await sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(msg), WebSocketMessageType.Binary, true, token));
                            //Task task = sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(msg), WebSocketMessageType.Binary, true, token));
                            //Task.WaitAll(task);
                        }
                        else
                        {
                            int offset = 0;
                            byte[] buffer = null;
                            while (msg.Length - offset > config.SendBufferSize)
                            {
                                buffer = new byte[config.SendBufferSize];
                                Buffer.BlockCopy(msg, offset, buffer, 0, buffer.Length);
                                await sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, token));
                                //Task t = sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, token));
                                //Task.WaitAll(t);
                                offset += buffer.Length;
                            }

                            buffer = new byte[msg.Length - offset];
                            await sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, token));
                            //Task task = sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, token));
                            //Task.WaitAll(task);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Web socket send fail");
                Trace.TraceError(ex.Message);
            }


            //if(message.Length > config.MaxIncomingMessageSize)
            //{
            //    throw new InvalidOperationException("Exceeds max message size.");
            //}

            //try
            //{

            //    if (message.Length <= config.SendBufferSize)
            //    {
            //        await sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Binary, true, token));
            //    }
            //    else
            //    {
            //        int offset = 0;
            //        byte[] buffer = null;
            //        while (message.Length - offset > config.SendBufferSize)
            //        {
            //            buffer = new byte[config.SendBufferSize];
            //            Buffer.BlockCopy(message, offset, buffer, 0, buffer.Length);
            //            await sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, false, token));

            //            offset += buffer.Length;
            //        }

            //        buffer = new byte[message.Length - offset];
            //        await sendQueue.Enqueue(() => client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Binary, true, token));
            //    }
            //}
            //catch(Exception ex)
            //{
            //    await SkunkLab.Diagnostics.Logging.Log.LogWarningAsync("Web socket client send fail.");
            //    await SkunkLab.Diagnostics.Logging.Log.LogErrorAsync(ex.Message);
            //}
            
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
