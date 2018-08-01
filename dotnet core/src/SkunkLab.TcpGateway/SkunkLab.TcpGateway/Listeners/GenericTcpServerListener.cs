using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.TcpGateway.Listeners
{
    public class GenericTcpServerListener : TcpServerListener
    {
        public GenericTcpServerListener(IPEndPoint localEP, int blockSize, int maxBufferSize, bool usePrefix, CancellationToken token)
            : this(localEP.Address, localEP.Port, blockSize, maxBufferSize, usePrefix, token)
        {
        }

        public GenericTcpServerListener(IPAddress address, int port, int blockSize, int maxBufferSize, bool usePrefix, CancellationToken token)
        {
            serverIP = address;
            serverPort = port;
            this.blockSize = blockSize;
            this.maxBufferSize = maxBufferSize;
            prefixed = usePrefix;
            listener = new TcpListener(address, port);
            listener.ExclusiveAddressUse = false;
            this.token = token;

        }

        private IPAddress serverIP;
        private int serverPort;
        private TcpListener listener;
        private CancellationToken token;
        private int blockSize;
        private int maxBufferSize;
        private bool prefixed;
        private Dictionary<string, IChannel> channelContainer;

        public async override Task StartAsync()
        {
            listener.ExclusiveAddressUse = false;
            listener.Start();

            while (!token.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                client.LingerState = new LingerOption(true, 0);
                client.NoDelay = true;
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.Client.UseOnlyOverlappedIO = true;

                //create the TCP channel
                IChannel channel = new TcpServerChannel(client, maxBufferSize, token);
                channel.OnReceive += Channel_OnReceive;
                channel.OnClose += Channel_OnClose;
                channel.OnOpen += Channel_OnOpen;
                channel.OnError += Channel_OnError;

                await channel.OpenAsync();
                await channel.ReceiveAsync();

                channelContainer.Add(channel.Id, channel);

                
                

            }
        }

       

        public async override Task StopAsync()
        {
            throw new NotImplementedException();
        }

        #region Channel events

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {

        }

        #endregion
    }
}
