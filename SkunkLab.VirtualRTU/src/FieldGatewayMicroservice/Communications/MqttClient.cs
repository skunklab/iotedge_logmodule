using FieldGatewayMicroservice.Connections;
using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VirtualRtu.Common.Configuration;

namespace FieldGatewayMicroservice.Communications
{
    public class MqttClient 
    {
        protected MqttClient(IssuedConfig config)
        {
            this.config = config;
        }

        public static MqttClient Create()
        {
            if(instance == null)
            {
                throw new CommunicationsException("MqttClient singleton must already exist if config not specfiied in Create method.");
            }
            else
            {
                return instance;
            }
        }

        public static MqttClient Create(IssuedConfig config)
        {
            if(instance == null)
            {
                instance = new MqttClient(config);
            }

            return instance;
        }

        private static MqttClient instance;

        public event System.EventHandler<MessageEventArgs> OnMessage;
        public event System.EventHandler<MqttErrorEventArgs> OnError;
        private IssuedConfig config;
        private PiraeusMqttClient client;
        private IChannel channel;
        private CancellationTokenSource channelCancelToken;

        public async Task<ConnectAckCode> ConnectAsync()
        {
            try
            {
                if (channel != null)
                {
                    channel.Dispose();
                }

                channelCancelToken = new CancellationTokenSource();
                channel = ChannelFactory.Create(false, IPAddress.Parse(config.Hostname), config.Port, config.PskIdentity, Convert.FromBase64String(config.PSK), 2048, 2048 * 100, channelCancelToken.Token);
                MqttConfig mqttConfig = new MqttConfig(180);

                client = new PiraeusMqttClient(mqttConfig, channel);
                client.OnChannelError += Client_OnChannelError;

                return await client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", config.SecurityToken, 180);
            }
            catch(Exception ex)
            {
                Console.WriteLine("MQTT client failed to connect - '{0}", ex.Message);
                client = null;
                channel = null;
                return ConnectAckCode.ServerUnavailable;
            }
        }

        public async Task SubscribeAsync()
        {
            await client.SubscribeAsync(config.Resources.RtuInputResource, QualityOfServiceLevelType.AtMostOnce, Input);
        }

        public async Task PublishAsync(byte[] message)
        {
            
            if(client == null || !client.IsConnected)
            {
                ConnectAckCode code = await ConnectAsync();
                if (code == ConnectAckCode.ConnectionAccepted)
                {
                    await SubscribeAsync();
                }
                else
                {
                    Console.WriteLine("Cannot perform MQTT publish at this time due to lost connection.");
                    throw new CommunicationsException("Lost MQTT connection.");
                }
            }

            await client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, config.Resources.RtuOutputResource, Constants.CONTENT_TYPE, message);
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await client.DisconnectAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine("MQTT disconnect exception - '{0}'", ex.Message);
            }

            client = null;
            channel = null;
        }


        private void Input(string resourceUriString, string contentType, byte[] message)
        {
            //signal a message received
            OnMessage?.Invoke(this, new MessageEventArgs(message));
        }

        private void Client_OnChannelError(object sender, ChannelErrorEventArgs args)
        {
            OnError?.Invoke(this, new MqttErrorEventArgs(args.Error));
        }

    }

    
}
