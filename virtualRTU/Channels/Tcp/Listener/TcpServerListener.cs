using SkunkLab.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Channels.Tcp.Listener
{
    public class TcpServerListener
    {
        private IPAddress serverIP;
        private int serverPort;
        private TcpListener listener;
        private CancellationToken token;
        private Dictionary<string, CommunicationAdapter> container;
        private Type commAdapter;
        private object[] parameters;

        public TcpServerListener(IPAddress address, int port, CancellationToken token)
        {
            container = new Dictionary<string, CommunicationAdapter>();
            serverIP = address;
            serverPort = port;
            listener = new TcpListener(address, port);
            listener.ExclusiveAddressUse = false;
            this.token = token;
        }

        public TcpServerListener(IPAddress address, int port, Type comAdapter, CancellationToken token, params object[] parameters)
        {
            container = new Dictionary<string, CommunicationAdapter>();
            serverIP = address;
            serverPort = port;
            commAdapter = comAdapter;
            listener = new TcpListener(address, port);
            listener.ExclusiveAddressUse = false;
            this.token = token;
            this.parameters = parameters;
        }

        public async Task StartAsync()
        {
            listener.ExclusiveAddressUse = false;
            listener.Start();

            Console.WriteLine("Listener started on IP {0} Port {1}", serverIP.ToString(), serverPort);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    client.LingerState = new LingerOption(true, 0);
                    client.NoDelay = true;
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Client.UseOnlyOverlappedIO = true;

                    ManageConnection(client);
                }
                catch (Exception ex)
                {
                    //OnError?.Invoke(this, new ServerFailedEventArgs("TCP", serverPort));
                    //await Log.LogErrorAsync("TCP server listener error {0}", ex.Message);
                }
            }
        }

        private void ManageConnection(TcpClient client)
        {
            IChannel channel = ChannelFactory.Create(false, client, 1024, 2048, token);
            CommunicationAdapter adapter = null;
            if (parameters == null)
            {

                adapter = (CommunicationAdapter)Activator.CreateInstance(commAdapter);
            }
            else
            {
                adapter = (CommunicationAdapter)Activator.CreateInstance(commAdapter, parameters);
            }

            adapter.OnClose += CommunicationAdapter_OnClose;
            adapter.OnError += CommunicationAdapter_OnError;
            adapter.Channel = channel;
            adapter.Init();
            container.Add(channel.Id, adapter);
        }

        private void CommunicationAdapter_OnError(object sender, ComAdapterEventArgs e)
        {
            Console.WriteLine("Communcation adapter error '{0}' - '{1}' - '{2}'", e.ChannelId, e.Error.Message, DateTime.Now);
        }

        private void CommunicationAdapter_OnClose(object sender, ComAdapterEventArgs e)
        {
            if(container.ContainsKey(e.ChannelId))
            {
                CommunicationAdapter adapter = container[e.ChannelId];
                container.Remove(e.ChannelId);
                adapter.Dispose();
            }
        }

        
    }
}
