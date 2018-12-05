using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Adapters
{
    public class PiraeusClient
    {
        private IChannel channel;
        private CancellationTokenSource source;
        private PiraeusMqttClient client;
        private const string CONTENT_TYPE = "applicaton/octet-stream";
        private VRtuConfig config;

        public event System.EventHandler<SubscriptionEventArgs> OnReceive;

        public PiraeusClient(VRtuConfig config)
        {
            this.config = config;
            source = new CancellationTokenSource();
            channel = ChannelFactory.Create(false, config.Hostname, config.Port, config.PskIdentity, config.Psk, config.BlockSize, config.MaxBufferSize, source.Token);
            MqttConfig mqttConfig = new MqttConfig(config.KeepAliveInterval);
            client = new PiraeusMqttClient(mqttConfig, channel);     
            
        }


        public async Task ConnectAsync()
        {
            Task<ConnectAckCode> task = client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", config.SecurityToken, config.KeepAliveInterval);
            Task.WaitAll(task);
            if (task.Result != ConnectAckCode.ConnectionAccepted)
            {
                throw new SecurityException("MQTT client failed to connect.");
            }

            await Task.CompletedTask;
        }

        public async Task DisconnectAsync()
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync();
            }
        }

        public async Task SubscribeAsync(string resourceUriString)
        {
            await client.SubscribeAsync(resourceUriString, QualityOfServiceLevelType.AtMostOnce, SubscriptionResult);
        }

        public async Task SendAsync(string resourceUriString, byte[] message)
        {
            await client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, resourceUriString, CONTENT_TYPE, message);
        }

        private void SubscriptionResult(string resourceUriString, string contentType, byte[] message)
        {
            //raise receive event
            OnReceive?.Invoke(this, new SubscriptionEventArgs(resourceUriString, contentType, message));
        }
    }
}
