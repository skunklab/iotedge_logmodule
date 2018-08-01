using Piraeus.Clients.Mqtt;
using PiraeusClientModule.Channels;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PiraeusClientModule.Clients.ModBus
{
    public class ModBusClient
    {
        public ModBusClient(ModBusClientConfig config)
        {
            this.config = config;
        }
        private ModBusClientConfig config;
        private CancellationTokenSource cts;
        private IChannel channel;
        private PiraeusMqttClient client;
        private bool warned;

        public event System.EventHandler<ModBusMessageEventArgs> OnCloudMessage;

        /// <summary>
        /// This is method the receives the ModBus request from the cloud.
        /// You must forward the binary payload to the ModBus Protocol Adapter communicating with the RTU.
        /// </summary>
        /// <param name="resourceUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="payload"></param>
        void ObserveModBusRequest(string resourceUriString, string contentType, byte[] payload)
        {
            //raise event that message was received from cloud
            OnCloudMessage?.Invoke(this, new ModBusMessageEventArgs(payload));
        }

        public async Task ConnectAsync()
        {
            try
            {
                cts = new CancellationTokenSource();
                SkunkLab.Protocols.Mqtt.MqttConfig config = new SkunkLab.Protocols.Mqtt.MqttConfig();

                IPAddress address = string.IsNullOrEmpty(this.config.Hostname) ? this.config.RemoteAddress : Dns.GetHostEntry(this.config.Hostname).AddressList[0];
                channel = GetTcpChannel(address, this.config.MqttPort, this.config.PskIdentity, this.config.PSK, this.config.TcpBlockSize, this.config.TcpMaxBufferSize, cts.Token);

                client = new PiraeusMqttClient(config, channel);
                client.OnChannelError += Client_OnChannelError;
                channel.OnClose += Channel_OnClose;

                client.RegisterTopic(this.config.ModBusRequestResource, ObserveModBusRequest);

                ConnectAckCode code = await client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", this.config.SecurityToken, 120);

                if (code != ConnectAckCode.ConnectionAccepted)
                {
                    await channel.CloseAsync();
                    channel = null;
                }
                else
                {
                    await SubscribeAsync();
                }

                Trace.TraceInformation("ModBus-Cloud client started {0}", DateTime.Now);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Fault starting client '{0}' ", DateTime.Now);
                Trace.TraceError(ex.Message);
                if (!warned)
                {
                    Trace.TraceError(ex.StackTrace);
                    warned = true;
                }
            }
        }

        public async Task SendAsync(byte[] message)
        {
            await client.PublishAsync(QualityOfServiceLevelType.AtLeastOnce, config.ModBusResponseResource, "application/octet-stream", message);
        }
        

        private async Task SubscribeAsync()
        {
            await client.SubscribeAsync(this.config.ModBusRequestResource, QualityOfServiceLevelType.AtLeastOnce, ObserveModBusRequest);
        }

        private IChannel GetTcpChannel(IPAddress remoteEndpoint, int port, string pskIdentity, byte[] psk, int blockSize, int maxBufferSize, CancellationToken token)
        {
            return ChannelFactory.Create(false, remoteEndpoint, port, null, pskIdentity, psk, blockSize, maxBufferSize, token);
        }

        private  void Client_OnChannelError(object sender, ChannelErrorEventArgs args)
        {
            if(channel.IsConnected)
            {
                Task task = channel.CloseAsync();
                Task.WhenAll(task);
            }
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            //the channel is closed -- restart
            if(config.KeepChannelOpen)
            {
                Task task = Task.Delay(Convert.ToInt32(this.config.ChannelOpenDelay));
                Task.WaitAll(task);

                Trace.TraceWarning("Starting ModBus-Cloud client {0}", DateTime.Now);
                Task connectTask = ConnectAsync();
                Task.WhenAll(connectTask);
            }
        }
    }
}
