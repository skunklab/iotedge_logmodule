using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.VirtualRtu.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SkunkLab.Protocols.Mqtt;

namespace SkunkLab.VirtualRtu.Server
{
    public class Listener
    {
        public Listener()
        {
            container = new Dictionary<string, VirtualRtuAdapter>();
        }

        public event System.EventHandler<VRtuListenerErrorEventArgs> OnError;
        private TcpListener listener;
        private VRtuConfig rtuConfig;
        private MqttConfig mqttConfig;
        private RtuMap map;
        private Dictionary<string, VirtualRtuAdapter> container;
        private CancellationTokenSource src;
        private string pskCopy;
        public async Task StartAsync(VRtuConfig rtuConfig, MqttConfig mqttConfig, RtuMap map, CancellationToken token)
        {
            this.rtuConfig = rtuConfig;
            pskCopy = Convert.ToBase64String(rtuConfig.Psk);
            this.mqttConfig = mqttConfig;
            this.map = map;
            listener = new TcpListener(new IPEndPoint(GetIPAddress(System.Net.Dns.GetHostName()), 502));
            listener.ExclusiveAddressUse = false;
            listener.Start();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    client.LingerState = new LingerOption(true, 0);
                    client.NoDelay = true;
                    //client.ExclusiveAddressUse = false;
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    //client.Client.UseOnlyOverlappedIO = true;

                    ManageConnection(client);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new VRtuListenerErrorEventArgs(ex));
                }
            }
        }

        private void ManageConnection(TcpClient client)
        {
            src = new CancellationTokenSource();
            IChannel channel = ChannelFactory.Create(false, client, rtuConfig.BlockSize, rtuConfig.MaxBufferSize, src.Token);
                
            string id = channel.Id;
            VirtualRtuAdapter adapter = null;
            try
            {
                adapter = new VirtualRtuAdapter(rtuConfig, map, channel);
                adapter.OnError += Adapter_OnError;
                adapter.OnClose += Adapter_OnClose;

                Task task = channel.OpenAsync();
                Task.WaitAll(task);
                Task rtask = channel.ReceiveAsync();
                Task.WhenAll(rtask);
                container.Add(id, adapter);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WARNING: Error creating V-RTU adatper on {0}", id);
                Console.WriteLine("ERROR: V-RTU Creation Error {0}", ex.Message);
                //Trace.TraceError("V-RTU adapter error with '{0}'.", ex.Message);
                if(container.ContainsKey(id))
                {
                    VirtualRtuAdapter vadapter = container[id];
                    vadapter.Dispose();
                    container.Remove(id);
                }
                Console.WriteLine("WARNING: V-RTU adpater disposed on {0}", id);
            }
        }

        private void Adapter_OnClose(object sender, AdapterEventArgs e)
        {
            //remove the adapter
            try
            {
                Console.WriteLine("V-RTU Adapter closing on {0}", e.ChannelId);
                if (container.ContainsKey(e.ChannelId))
                {
                    VirtualRtuAdapter adapter = container[e.ChannelId];
                    adapter.Dispose();
                    container.Remove(e.ChannelId);
                }

                Console.WriteLine("V-RTU Adapter closing and disposed adapter on {0}", e.ChannelId);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fault in Adapter Close event on {0}", e.ChannelId);
                Console.WriteLine(ex.StackTrace);
                //Trace.TraceError(ex.Message);
            }
        }

        private void Adapter_OnError(object sender, AdapterEventArgs e)
        {
            try
            {
                Console.WriteLine("WARNING: V-RTU Adapter error on {0}", e.ChannelId);
                Console.WriteLine("ERROR: V-RTU Adapter error {0}", e.Error.Message);
                Console.WriteLine("****** STACK TRACE ******");
                Console.WriteLine(e.Error.StackTrace);
                Console.WriteLine("****** END ******");
                //error in adapter, dispose it and remove from container
                if (container.ContainsKey(e.ChannelId))
                {
                    Console.WriteLine("Disposing channel {0}", e.ChannelId);
                    VirtualRtuAdapter adapter = container[e.ChannelId];
                    adapter.Dispose();
                    container.Remove(e.ChannelId);
                }
                Console.WriteLine("WARNING: V-RTU Adapter disposed on {0}", e.ChannelId);

            }
            catch(Exception ex)
            {
                Console.WriteLine("Fault in Adapter Error event.");
                Console.WriteLine(ex.StackTrace);
                Trace.TraceError(ex.Message);
            }
        }

        public async Task StopAsync()
        {
            string[] keys = container.Keys.ToArray();
            foreach (string key in keys)
            {
                if (container.ContainsKey(key))
                {
                    VirtualRtuAdapter adapter = container[key];
                    adapter.Dispose();
                    container.Remove(key);
                }
            }

            await Task.CompletedTask;
        }

        private IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }

    }
}
