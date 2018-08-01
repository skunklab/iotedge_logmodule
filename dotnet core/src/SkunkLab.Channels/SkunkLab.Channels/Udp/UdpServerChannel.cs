using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Channels.Udp
{
    public class UdpServerChannel : UdpChannel
    {

        public UdpServerChannel(UdpClient listener, IPEndPoint remoteEP, CancellationToken token)
        {
            Id = "udp-" + Guid.NewGuid().ToString();
            this.client = listener;
            this.remoteEP = remoteEP;
            this.token = token;
            
        }

        
        private IPEndPoint remoteEP;
        private UdpClient client;
        private ChannelState _state;
        private CancellationToken token;
        private bool disposedValue;

        public override event EventHandler<ChannelReceivedEventArgs> OnReceive;
        public override event EventHandler<ChannelCloseEventArgs> OnClose;
        public override event EventHandler<ChannelOpenEventArgs> OnOpen;
        public override event EventHandler<ChannelErrorEventArgs> OnError;
        public override event EventHandler<ChannelStateEventArgs> OnStateChange;


        public override string Id { get; internal set; }

        public override bool RequireBlocking
        {
            get { return false; }
        }

        public override string TypeId { get { return "UDP"; } }

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
                State = ChannelState.Open; //the channel is already open by the listener

                OnOpen?.Invoke(this, new ChannelOpenEventArgs(Id, null));
            }
            catch(Exception ex)
            {
                Trace.TraceError("UDP server channel {0} open error {1}", Id, ex.Message);
                State = ChannelState.Aborted;
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }

            await Task.CompletedTask;
            
        }

        public override async Task ReceiveAsync()
        {

            //nothing implemented here because the listener will call AddMessageAsync and raise OnReceive
            //We do bind the remote endpoint to call SendAsync to the connected UDP client.

            await Task.CompletedTask;

        }

        public override async Task AddMessageAsync(byte[] message)
        {
            //Raise the event received from the Protocol Adapter on the gateway
            OnReceive?.Invoke(this, new ChannelReceivedEventArgs(Id, message));
            await Task.CompletedTask;
        }

        public override async Task CloseAsync()
        {
            //nothing to do here because closing the client is closing the listener to all channels 
            //connected to the listener.
            State = ChannelState.Closed;
            OnClose?.Invoke(this, new ChannelCloseEventArgs(Id)); //listener is closing
            await Task.CompletedTask;
        }

        protected void Disposing(bool dispose)
        {
            if (dispose & !disposedValue)
            {
                //client.Close(); cannot close client because is universal listener.
                //put need this because of  IChannel interface
                disposedValue = true;
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        public override async Task SendAsync(byte[] message)
        {
            try
            {               
                await client.SendAsync(message, message.Length, remoteEP);
            }
            catch(Exception ex)
            {             
                Trace.TraceError("UDP server channel {0} send error {1}", Id, ex.Message);
                OnError?.Invoke(this, new ChannelErrorEventArgs(Id, ex));
            }
        }
    }
}
