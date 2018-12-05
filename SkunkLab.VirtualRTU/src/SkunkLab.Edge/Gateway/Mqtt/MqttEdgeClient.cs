using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VirtualRtu.Common.Configuration;

namespace SkunkLab.Edge.Gateway.Mqtt
{
    public class MqttEdgeClient
    {
        public event System.EventHandler<MqttReceiveEventArgs> OnReceive;
        public event System.EventHandler<CommunicationsErrorEventArgs> OnError;

        private IssuedConfig config;
        private PiraeusMqttClient client;
        private IChannel channel;
        private CancellationTokenSource src;

        public MqttEdgeClient(IssuedConfig config)
        {
            this.config = config;
        }

        public async Task ConnectAsync()
        {
            src = new CancellationTokenSource();
            MqttConfig mqttConfig = new MqttConfig(60.0);
            channel = ChannelFactory.Create(false, IPAddress.Parse(config.Hostname), config.Port, config.PskIdentity, Convert.FromBase64String(config.PSK), 2048, 4096 * 100, src.Token);

            client = new PiraeusMqttClient(mqttConfig, channel);
            client.OnChannelError += Client_OnChannelError;

            ConnectAckCode code = await client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", config.SecurityToken, 90);

            if(code == ConnectAckCode.ConnectionAccepted)
            {
                try
                {
                    await client.SubscribeAsync(config.Resources.RtuInputResource, QualityOfServiceLevelType.AtMostOnce, ReceiveMessage);
                }
                catch
                {
                    src.Cancel();
                    throw new CommunicationsException(String.Format("Cannot subscribe to resource '{0}'.", config.Resources.RtuInputResource));
                }
            }
            else
            {
                throw new CommunicationsException(String.Format("MQTT client connected with invalid code '{0}'.", code));
            }
        }

        public async Task DisconnectAsync()
        {
            if(client != null)
            {
                try
                {
                    await client.DisconnectAsync();
                }
                catch                
                { }
            }
        }

        public async Task SendAsync(byte[] message)
        {            
            try
            {
                if (client != null && client.IsConnected)
                {
                    await client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, config.Resources.RtuOutputResource, "application/octet-stream", message);
                }
            }
            catch
            {
                src.Cancel();
                OnError?.Invoke(this, new CommunicationsErrorEventArgs(new CommunicationsException(String.Format("Cannot send to resource '{0}'", config.Resources.RtuOutputResource))));
            }
        }

        private void Client_OnChannelError(object sender, ChannelErrorEventArgs args)
        {
            //fatal error
            src.Cancel();
            OnError?.Invoke(this, new CommunicationsErrorEventArgs(args.Error));
        }

        private void ReceiveMessage(string resourceUriString, string contentType, byte[] message)
        {
            //raise event to signal a message receive to send to modbus adapter
            OnReceive?.Invoke(this, new MqttReceiveEventArgs(resourceUriString, contentType, message));
        }
    }
}
