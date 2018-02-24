using SkunkLab.Channels;
using SkunkLab.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Clients
{
    public class Server
    {
        public Server(IPAddress address, int port)
        {

            listener = new TcpServerListener(address, port, CancellationToken.None);
            

            //client = new TcpClient(new System.Net.IPEndPoint(address, port));
            //channel = TcpChannel.Create(client, CancellationToken.None);
            //channel.OnClose += Channel_OnClose;
            //channel.OnError += Channel_OnError;
            //channel.OnStateChange += Channel_OnStateChange;
            //channel.OnReceive += Channel_OnReceive;
            //channel.OnOpen += Channel_OnOpen;
        }

        private IChannel channel;
        private TcpClient client;
        private TcpServerListener listener;

        public async Task OpenAsync()
        {
            await listener.StartAsync();

            //await channel.OpenAsync();
        }

        public async Task SendAsync(byte[] message)
        {
            await channel.SendAsync(message);
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs args)
        {
            Console.WriteLine("Channel open");
            Task task = channel.ReceiveAsync();
            Task.WhenAll(task);
        }



        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            string message = Encoding.UTF8.GetString(args.Message);
            Console.WriteLine(message);
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            Console.WriteLine("State Change {0}", args.State);
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            Console.WriteLine("Error {0}", args.Error.Message);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            Console.WriteLine("Channel closed");
        }
    }
}
