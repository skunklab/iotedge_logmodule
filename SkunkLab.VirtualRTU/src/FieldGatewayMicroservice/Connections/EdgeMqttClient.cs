//using Piraeus.Clients.Mqtt;
//using SkunkLab.Channels;
//using SkunkLab.Edge.Gateway;
//using SkunkLab.Protocols.Mqtt;
//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace FieldGatewayMicroservice.Connections
//{
//    public class EdgeMqttClient
//    {
//        public event System.EventHandler<MqttErrorEventArgs> OnError;
//        public event System.EventHandler<MessageEventArgs> OnMessage;


//        public EdgeMqttClient(string hostname, int port, string pskIdentity, byte[] psk)
//        {
//            this.hostname = hostname;
//            this.port = port;
//            this.pskIdentity = pskIdentity;
//            this.psk = psk;
//        }


//        private PiraeusMqttClient client;
//        private CancellationTokenSource cts;
//        private string hostname;
//        private int port;
//        private string pskIdentity;
//        private byte[] psk;
//        private int blockSize = 1024;
//        private int maxBufferSize = 4 * 4096;

//        private IChannel channel;

//        public async Task ConnectAsync(string securityToken)
//        {
//            if (channel != null)
//            {
//                channel.Dispose();
//                channel = null;
//            }

//            if (client != null)
//            {
//                client = null;
//            }

//            try
//            {
//                cts = new CancellationTokenSource();
//                channel = ChannelFactory.Create(false, IPAddress.Parse(hostname), port, pskIdentity, psk, blockSize, maxBufferSize, cts.Token);

//                MqttConfig config = new MqttConfig(90);
//                client = new Piraeus.Clients.Mqtt.PiraeusMqttClient(config, channel);
//                client.OnChannelError += Client_OnChannelError;

//                ConnectAckCode code = await client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", securityToken, 90);

//                if (code != ConnectAckCode.ConnectionAccepted)
//                {
//                    throw new CommunicationsException(String.Format("Invalid Mqtt ACK code '{0}'", code));
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new CommunicationsException("Fault on MQTT connect. See inner exceptions", ex);
//            }
//        }

//        private void Client_OnChannelError(object sender, ChannelErrorEventArgs args)
//        {
//            OnError?.Invoke(this, new MqttErrorEventArgs(args.Error));
//        }

//        public async Task Subscribe(string resourceUriString)
//        {
//            if (client == null)
//            {
//                throw new CommunicationsException("MQTT client is null");
//            }

//            if (!client.IsConnected)
//            {
//                throw new CommunicationsException("MQTT client is disconnected.");
//            }

//            try
//            {
//                await client.SubscribeAsync(resourceUriString, QualityOfServiceLevelType.AtMostOnce, Input);
//            }
//            catch (Exception ex)
//            {
//                throw new CommunicationsException("Fault on MQTT subscribe.  See inner exceptions", ex);
//            }
//        }

//        public async Task PublishAsync(string resourceUriString, byte[] payload)
//        {
//            if (client == null)
//            {
//                throw new CommunicationsException("MQTT client is null");
//            }

//            if (!client.IsConnected)
//            {
//                throw new CommunicationsException("MQTT client is disconnected.");
//            }

//            try
//            {
//                await client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, resourceUriString, "application/octet-stream", payload);
//            }
//            catch (Exception ex)
//            {
//                throw new CommunicationsException("Fault on MQTT publish.  See inner exceptions", ex);
//            }
//        }

//        public async Task CloseAsync()
//        {
//            if (client == null)
//            {
//                return;
//            }

//            try
//            {
//                await client.DisconnectAsync();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Fault during disconnect '{0}'", ex.Message);
//            }

//            client = null;
//        }


//        private void Input(string resourceUriString, string contentType, byte[] message)
//        {
//            //raise event to signal new input
//            OnMessage.Invoke(this, new MessageEventArgs(message));
//        }
//    }
//}
