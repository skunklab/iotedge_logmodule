using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Udp
{
    public class UdpClientChannel : UdpChannel
    {

        public UdpClientChannel(int localPort, IPEndPoint remoteEP, CancellationToken token)
        {
            Port = localPort;
            this.remoteEP = remoteEP;
            this.token = token;
            Id = "udp-" + Guid.NewGuid().ToString();
        }

        public UdpClientChannel(int localPort, string hostname, int port, CancellationToken token)
        {
            this.Port = localPort;
            this.hostname = hostname;
            this.port = port;
            this.token = token;
            Id = "udp-" + Guid.NewGuid().ToString();
        }

        private CancellationToken token;
        private UdpClient client;
        private IPEndPoint remoteEP;
        private string hostname;
        private int port;
        private ChannelState _state;
        private bool disposedValue;


        public override string Id { get; internal set; }

        public override bool RequireBlocking
        {
            get { return false; }
        }

        public override string TypeId { get { return "UDP"; } }
        public override bool IsConnected
        {
            get
            {
                if(disposedValue || client == null || client.Client == null)
                {
                    return false;
                }
                else
                {
                    return client.Client.Connected;
                }
            }
        }

        public override int Port { get; internal set; }
        public override ChannelState State
        {
            get
            {
                return _state;
            }
            internal set
            {
                if (value != _state)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }

                _state = value;
            }
        }
        public override bool IsEncrypted { get; internal set; }
        public override bool IsAuthenticated { get; internal set; }

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;

        

        public override async Task AddMessageAsync(byte[] message)
        {
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await Task.CompletedTask;
        }

        public override async Task CloseAsync()
        {
            client.Close();
            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));
            await Task.CompletedTask;
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposedValue)
            {
                if (client != null && IsConnected)
                {
                    client.Close();
                }

                client.Dispose();
                disposedValue = true;
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        public override async Task OpenAsync()
        {
            State = ChannelState.Connecting;
            client = new UdpClient(Port);
            client.DontFragment = true;

            try
            {
                if (!String.IsNullOrEmpty(hostname))
                {
                    client.Connect(hostname, port);
                }
                else
                {
                    client.Connect(remoteEP);
                }

                State = ChannelState.Open;
               
                OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
            }
            catch(Exception ex)
            {
                client = null;
                Trace.TraceError("UDP client channel {0} open error {1}", Id, ex.Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }

            await Task.CompletedTask;
        }

        public override async Task ReceiveAsync()
        {
            
            while(IsConnected && !token.IsCancellationRequested)
            {
                try
                {  
                    UdpReceiveResult result = await client.ReceiveAsync();                    
                    OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, result.Buffer));
                }
                catch(Exception ex)
                {
                    Trace.TraceError("UDP client channel {0} receive error {1}", Id, ex.Message);
                    OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
                    break;
                }
            }

            await CloseAsync();
        }

        public override async Task SendAsync(byte[] message)
        {
            try
            {               
                if (remoteEP == null)
                {
                    await client.SendAsync(message, message.Length);
                }
                else
                {
                    await client.SendAsync(message, message.Length);
                }

                
            }
            catch(Exception ex)
            {
                Trace.TraceError("UDP client channel {0} send error {1}", Id, ex.Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }
        }
    }
}
