using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SkunkLab.Channels;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Protocols.Mqtt.Handlers;
using SkunkLab.Protocols.Utilities;

namespace Piraeus.Clients.Mqtt
{
    public delegate void MqttClientChannelStateHandler(object sender, ChannelStateEventArgs args);
    public delegate void MqttClientChannelErrorHandler(object sender, ChannelErrorEventArgs args);
    //public delegate void EventHandler<>
    
    public class PiraeusMqttClient
    {
        /// <summary>
        /// Creates a new instance of MQTT client.
        /// </summary>
        /// <param name="config">MQTT configuration.</param>
        /// <param name="channel">Channel for communications.</param>
        /// <param name="dispatcher">Optional custom dispatcher to receive published messages.</param>
        public PiraeusMqttClient(MqttConfig config, IChannel channel, IDispatch dispatcher = null)
        {
            this.dispatcher = dispatcher != null ? dispatcher : new GenericDispatcher();
            timeoutMilliseconds = config.MaxTransmitSpan.TotalMilliseconds;
            session = new MqttSession(config);
            session.OnConnect += Session_OnConnect;
            session.OnDisconnect += Session_OnDisconnect;
            session.OnRetry += Session_OnRetry;

            this.channel = channel;
            this.channel.OnReceive += Channel_OnReceive;
            this.channel.OnClose += Channel_OnClose;
            this.channel.OnError += Channel_OnError;
            this.channel.OnStateChange += Channel_OnStateChange;
        }

        

        public event MqttClientChannelStateHandler OnChannelStateChange;
        public event MqttClientChannelErrorHandler OnChannelError;

        private IChannel channel;
        private MqttSession session;
        private ConnectAckCode? code;
        private double timeoutMilliseconds;
        private IDispatch dispatcher;

        /// <summary>
        /// Register a topic that already has a subscription.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="action"></param>
        public void RegisterTopic(string topic, Action<string, string, byte[]> action)
        {
            Uri uri = new Uri(topic.ToLower(CultureInfo.InvariantCulture));
            dispatcher.Register(uri.ToString(), action);
        }

        /// <summary>
        /// Unregister a topic that has a subscription
        /// </summary>
        /// <param name="topic"></param>
        public void UnregisterTopic(string topic)
        {
            Uri uri = new Uri(topic.ToLower(CultureInfo.InvariantCulture));
            dispatcher.Unregister(uri.ToString());
        }

        /// <summary>
        /// Connect to Piraeus using MQTT
        /// </summary>
        /// <param name="clientId">MQTT client ID</param>
        /// <param name="username">MQTT username.  Piraeus uses the security token type used in the MQTT password.</param>
        /// <param name="password">MQTT passowrd.  Piraeus uses the security token that will be authenticated (JWT, SWT, X509).</param>
        /// <param name="keepaliveSeconds">Number of seconds that keep alive should fire when messages not sent during the duration.</param>
        /// <returns></returns>
        public async Task<ConnectAckCode> ConnectAsync(string clientId, string username, string password, int keepaliveSeconds)
        {
            code = null;

            ConnectMessage msg = new ConnectMessage(clientId, username, password, keepaliveSeconds, true);

            if (!channel.IsConnected)
            {
                await channel.OpenAsync();                
            }

            await channel.SendAsync(msg.Encode());

            DateTime expiry = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while(!code.HasValue)
            {
                await Task.Delay(10);
                if(DateTime.UtcNow > expiry)
                {
                    throw new TimeoutException("MQTT connection timed out.");
                }
            }

            return code.Value;
        }

        /// <summary>
        /// Disconnect from Piraeus
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            DisconnectMessage msg = new DisconnectMessage();
            await channel.SendAsync(msg.Encode());
        }

        /// <summary>
        /// MQTT publish to a topic.
        /// </summary>
        /// <param name="qos"></param>
        /// <param name="topicUriString"></param>
        /// <param name="contentType"></param>
        /// <param name="data"></param>
        /// <param name="indexes"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task Publish(QualityOfServiceLevelType qos, string topicUriString, string contentType, byte[] data, IEnumerable<KeyValuePair<string, string>> indexes = null, string messageId = null)
        {
            UriBuilder builder = new UriBuilder(topicUriString);
            builder.Query = messageId == null ? String.Format("{0}={1}", SkunkLab.Protocols.Utilities.QueryStringConstants.CONTENT_TYPE, contentType) : String.Format("{0}={1}&{2}={3}", QueryStringConstants.CONTENT_TYPE, contentType, QueryStringConstants.MESSAGE_ID, messageId);
            
            PublishMessage msg = new PublishMessage(false, qos, false, 0, builder.ToString(), data);
            if(qos != QualityOfServiceLevelType.AtMostOnce)
            {
                msg.MessageId = session.NewId();
                session.Quarantine(msg);
            }

            await channel.SendAsync(msg.Encode());
        }

        /// <summary>
        /// MQTT subscribe to a topic.  Remember, these subscriptionns are ephemeral.
        /// </summary>
        /// <param name="topicUriString"></param>
        /// <param name="qos"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(string topicUriString, QualityOfServiceLevelType qos, Action<string, string, byte[]> action)
        {
            Dictionary<string, QualityOfServiceLevelType> dict = new Dictionary<string, QualityOfServiceLevelType>();
            dict.Add(topicUriString, qos);
            dispatcher.Register(topicUriString, action);
            SubscribeMessage msg = new SubscribeMessage(session.NewId(), dict);
            await channel.SendAsync(msg.Encode());
        }

        /// <summary>
        /// MQTT subscribe to a topic.  Remember, these subscriptions are ephemeral.
        /// </summary>
        /// <param name="subscriptions"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(Tuple<string, QualityOfServiceLevelType, Action<string, string, byte[]>>[] subscriptions)
        {
            Dictionary<string, QualityOfServiceLevelType> dict = new Dictionary<string, QualityOfServiceLevelType>();

            foreach(var tuple in subscriptions)
            {
                dict.Add(tuple.Item1, tuple.Item2);
                dispatcher.Register(tuple.Item1, tuple.Item3);
            }
            
            SubscribeMessage msg = new SubscribeMessage(session.NewId(), dict);
            await channel.SendAsync(msg.Encode());
        }

        /// <summary>
        /// Unsubscribe from an ephemeral subscription.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(string topic)
        {
            UnsubscribeMessage msg = new UnsubscribeMessage(session.NewId(), new string[] { topic });           
            await channel.SendAsync(msg.Encode());
            dispatcher.Unregister(topic);
        }

        /// <summary>
        /// Unsubscribe from an ephemeral subscription.
        /// </summary>
        /// <param name="topics"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(IEnumerable<string> topics)
        {
            UnsubscribeMessage msg = new UnsubscribeMessage(session.NewId(), topics);
            await channel.SendAsync(msg.Encode());

            foreach(var topic in topics)
            {
                dispatcher.Unregister(topic);
            }
        }

        #region Channel Events
        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            MqttMessage msg = MqttMessage.DecodeMessage(args.Message);

            MqttMessageHandler handler = MqttMessageHandler.Create(session, msg);
            Task<MqttMessage> task = handler.ProcessAsync();
            Task.WhenAll<MqttMessage>(task);
            MqttMessage response = task.Result;

            if(response != null)
            {
                Task task2 = channel.SendAsync(response.Encode());
                Task.WhenAll(task2);
            }
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            OnChannelError?.Invoke(this, args);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            code = null;
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs args)
        {
            OnChannelStateChange?.Invoke(this, args);
        }

        #endregion

        #region Session Events
        private void Session_OnConnect(object sender, MqttConnectionArgs args)
        {
            code = args.Code;
        }
        private void Session_OnRetry(object sender, MqttMessageEventArgs args)
        {
            MqttMessage msg = args.Message;
            msg.Dup = true;
            Task task = channel.SendAsync(msg.Encode());
            Task.WhenAll(task);
        }

        private void Session_OnDisconnect(object sender, MqttMessageEventArgs args)
        {
            Task task = channel.CloseAsync();
            Task.WaitAll(task);
            channel.Dispose();
        }

        #endregion
    }
}
