using SkunkLab.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Clients
{
    public class Client
    {
        public Client()
        {
            channel = TcpChannel.Create(IPAddress.Parse("127.0.0.1"), 12345, CancellationToken.None);
            channel.OnClose += Channel_OnClose;
            channel.OnError += Channel_OnError;
            channel.OnStateChange += Channel_OnStateChange;
            channel.OnReceive += Channel_OnReceive;
            channel.OnOpen += Channel_OnOpen;            
        }

        public async Task SendAsync(byte[] message)
        {
            if(!channel.IsConnected)
            {
                throw new InvalidOperationException("Channel is not open.");
            }

            await channel.SendAsync(message);
        }

        public async Task OpenAsync()
        {
            await channel.OpenAsync();
            
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

        private IChannel channel;
    }
}
