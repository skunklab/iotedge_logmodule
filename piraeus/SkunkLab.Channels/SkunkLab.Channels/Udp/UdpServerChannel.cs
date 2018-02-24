using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Udp
{
    public class UdpServerChannel : UdpChannel
    {
        
        public UdpServerChannel(IPEndPoint localEP, IPEndPoint remoteEP, CancellationToken token)
        {
            this.localEP = localEP;
            this.remoteEP = remoteEP;
            Id = Guid.NewGuid().ToString();
            client = new UdpClient();
            client.DontFragment = true;
            client.ExclusiveAddressUse = false;
            this.token = token;
        }

        private IPEndPoint remoteEP;
        private IPEndPoint localEP;
        private UdpClient client;
        private ChannelState _state;
        private CancellationToken token;

        public override event ChannelCloseEventHandler OnClose;
        public override event ChannelErrorEventHandler OnError;
        public override event ChannelOpenEventHandler OnOpen;
        public override event ChannelReceivedEventHandler OnReceive;
        public override event ChannelStateEventHandler OnStateChange;
        public override event ChannelRetryEventHandler OnRetry;
        public override event ChannelSentEventHandler OnSent;

        public override string Id { get; internal set; }

        public override int Port { get; internal set; }

        public override bool IsEncrypted { get; internal set; }

        public override bool IsAuthenticated { get; internal set; }

        public override bool IsConnected
        {
            get
            {
                return ChannelState.Open == State;
            }
        }

        public override ChannelState State
        {
            get
            {
                return _state;
            }
            internal set
            {
                if(value != _state)
                {
                    OnStateChange?.Invoke(this, new ChannelStateEventArgs(Id, value));
                }

                _state = value;
            }
        }

        public override async Task OpenAsync()
        {
            try
            {
                State = ChannelState.Connecting;
                client.Client.Bind(localEP);
                OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
                State = ChannelState.Open;
            }
            catch(Exception ex)
            {
                State = ChannelState.Aborted;
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }

            await TaskDone.Done;
        }

        public override async Task ReceiveAsync()
        {
            //not implemented because UDP server must "Add Message" to the receive pipeline
            await TaskDone.Done;
        }

        public override async Task AddMessage(byte[] message)
        {
            //Raise the event received from the Protocol Adapter on the gateway
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await TaskDone.Done;
        }

        public override Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override Task SendAsync(byte[] message)
        {
            throw new NotImplementedException();
        }
    }
}
