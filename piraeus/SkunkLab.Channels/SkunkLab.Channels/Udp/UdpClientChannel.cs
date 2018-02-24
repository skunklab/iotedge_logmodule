using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Udp
{
    public class UdpClientChannel : UdpChannel
    {
        public UdpClientChannel(IPEndPoint localEP, IPEndPoint remoteEP, CancellationToken token)
            : this(localEP, null, -1, token)
        {
            this.remoteEP = remoteEP;
        }
        public UdpClientChannel(IPEndPoint localEP, string hostname, int port, CancellationToken token)
        {
            this.localEP = localEP;
            this.hostname = hostname;
            this.port = port;
            this.token = token;
            Id = Guid.NewGuid().ToString();
        }

        private CancellationToken token;
        private IPEndPoint localEP;
        private UdpClient client;
        private IPEndPoint remoteEP;
        private string hostname;
        private int port;
        private bool connected;

        
        public override string Id { get; internal set; }
        public override bool IsConnected { get; }

        public override int Port { get; internal set; }
        public override ChannelState State { get => throw new NotImplementedException(); internal set => throw new NotImplementedException(); }
        public override bool IsEncrypted { get; internal set; }
        public override bool IsAuthenticated { get; internal set; }

        public override event ChannelCloseEventHandler OnClose;
        public override event ChannelErrorEventHandler OnError;
        public override event ChannelOpenEventHandler OnOpen;
        public override event ChannelReceivedEventHandler OnReceive;
        public override event ChannelStateEventHandler OnStateChange;
        public override event ChannelRetryEventHandler OnRetry;
        public override event ChannelSentEventHandler OnSent;

        public override async Task AddMessage(byte[] message)
        {
            throw new NotImplementedException();
        }

        public override async Task CloseAsync()
        {
            connected = false;
            client.Close();
            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id));
            await TaskDone.Done;
        }

        public override void Dispose()
        {
            client.Close();
            client = null;
        }

        public override async Task OpenAsync()
        {           
            client = new UdpClient(localEP);
            client.DontFragment = true;
            client.ExclusiveAddressUse = false;
            OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
            await TaskDone.Done;
        }

        public override async Task ReceiveAsync()
        {
            while(client != null && !token.IsCancellationRequested)
            {
                UdpReceiveResult result = await client.ReceiveAsync();
                OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, result.Buffer));
            }
        }

        public override async Task SendAsync(byte[] message)
        {
            if(remoteEP == null)
            {
                await client.SendAsync(message, message.Length, hostname, port);
            }
            else
            {
                await client.SendAsync(message, message.Length, remoteEP);
            }

            OnSent?.Invoke(this, new ChannelSentEventArgs(Id, null));
        }
    }
}
