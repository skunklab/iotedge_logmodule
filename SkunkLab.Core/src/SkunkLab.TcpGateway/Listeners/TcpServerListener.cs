using Piraeus.Adapters;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SkunkLab.TcpGateway.Listeners
{
    public class TcpServerListener : TcpServerListenerBase
    {
        public TcpServerListener(IPEndPoint localEP, PiraeusConfig piraeusConfig, CancellationToken token = default(CancellationToken))
            : this(localEP.Address, localEP.Port, piraeusConfig, token)
        {
            
        }

        public TcpServerListener(IPAddress address, int port, PiraeusConfig piraeusConfig, CancellationToken token = default(CancellationToken))
        {
            serverIP = address;
            this.serverPort = port;
            this.piraeusConfig = piraeusConfig;
            this.token = token;
            this.token.Register(async () => await StopAsync());

            

        }

        public override event EventHandler<TcpServerErrorEventArgs> OnError;

        //public TcpServerListener(IPEndPoint localEP, int blockSize, int maxBufferSize, bool usePrefix, CancellationToken token)
        //    : this(localEP.Address, localEP.Port, blockSize, maxBufferSize, usePrefix, token)
        //{
        //}

        //public TcpServerListener(IPAddress address, int port, int blockSize, int maxBufferSize, bool usePrefix, CancellationToken token)
        //{
        //    serverIP = address;
        //    serverPort = port;
        //    this.blockSize = blockSize;
        //    this.maxBufferSize = maxBufferSize;
        //    prefixed = usePrefix;
        //    listener = new TcpListener(address, port);
        //    listener.ExclusiveAddressUse = false;
        //    this.token = token;
        //}

        private PiraeusConfig piraeusConfig;

        private readonly IPAddress serverIP;
        private readonly int serverPort;
        private TcpListener listener;
        private CancellationToken token;
        private BasicAuthenticator authn;
        private Dictionary<string, Tuple<ProtocolAdapter, CancellationTokenSource>> container;

        public async override Task StartAsync()
        {
            authn = new BasicAuthenticator();
            container = new Dictionary<string, Tuple<ProtocolAdapter, CancellationTokenSource>>();
            listener.ExclusiveAddressUse = false;
            listener.Start();
            
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
                catch(Exception ex)
                {
                    OnError?.Invoke(this, new TcpServerErrorEventArgs(piraeusConfig.Channels.Tcp.UseLengthPrefix ? "TCP" : "TCP2", serverPort, ex));
                }
            }
        }

        public async override Task StopAsync()
        {
            try
            {
                KeyValuePair<string, Tuple<ProtocolAdapter, CancellationTokenSource>>[] array = container.ToArray();
                foreach (var item in array)
                {
                    item.Value.Item1.Dispose();
                }
                container.Clear();
            }
            catch
            {

            }

            await Task.CompletedTask;
        }

        private void ManageConnection(TcpClient client)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(piraeusConfig, authn, client, cts.Token);
            container.Add(adapter.Channel.Id, new Tuple<ProtocolAdapter, CancellationTokenSource>(adapter, cts));
            adapter.OnError += Adapter_OnError;
            adapter.OnClose += Adapter_OnClose;
            adapter.Init();
            Task openTask = adapter.Channel.OpenAsync();
            Task.WhenAll(openTask);
            Task receiveTask = adapter.Channel.ReceiveAsync();
            Task.WhenAll(receiveTask);
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            try
            {
                if (container.ContainsKey(e.ChannelId))
                {
                    Tuple<ProtocolAdapter, CancellationTokenSource> tuple = container[e.ChannelId];
                    container.Remove(e.ChannelId);
                    tuple.Item1.Dispose(); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fault Adapter_OnClose TCP Gateway");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            try
            {
                if (container.ContainsKey(e.ChannelId))
                {
                    Tuple<ProtocolAdapter, CancellationTokenSource> tuple = container[e.ChannelId];
                    container.Remove(e.ChannelId);
                    tuple.Item1.Dispose();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fault Adapter_OnError TCP Gateway");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
            }
        }

      
    }
}
