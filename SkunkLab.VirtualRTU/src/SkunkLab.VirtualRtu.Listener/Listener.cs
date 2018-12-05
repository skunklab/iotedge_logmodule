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

namespace SkunkLab.VirtualRtu.Listener
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

        public async Task StartAsync(VRtuConfig rtuConfig, MqttConfig mqttConfig, RtuMap map, CancellationToken token)
        {
            this.rtuConfig = rtuConfig;
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
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Client.UseOnlyOverlappedIO = true;

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
            IChannel channel = new TcpServerChannel(client, rtuConfig.MaxBufferSize, CancellationToken.None);
            string id = channel.Id;
            VirtualRtuAdapter adapter = null;
            try
            {

                adapter = new VirtualRtuAdapter(rtuConfig, map, channel);
                adapter.OnError += Adapter_OnError;
               
                Task task = channel.OpenAsync();
                Task.WaitAll(task);
                Task rtask = channel.ReceiveAsync();
                Task.WhenAll(rtask);
                container.Add(id, adapter);
            }
            catch(Exception ex)
            {
                Trace.TraceError("V-RTU adapter error with '{0}'.", ex.Message);
                adapter.Dispose();
            }
        }

        private void Adapter_OnError(object sender, AdapterEventArgs e)
        {
            //error in adapter, dispose it and remove from container
            if(container.ContainsKey(e.ChannelId))
            {
                VirtualRtuAdapter adapter = container[e.ChannelId];
                adapter.Dispose();
                container.Remove(e.ChannelId);
            }
        }

        public async Task StopAsync()
        {
            string[] keys = container.Keys.ToArray();
            foreach(string key in keys)
            {
                if(container.ContainsKey(key))
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
