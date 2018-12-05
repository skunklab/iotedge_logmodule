using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Channels;
using SkunkLab.Protocols;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Protocols.Mqtt.Handlers;
using SkunkLab.Protocols.Utilities;

namespace Piraeus.Clients.Mqtt
{
    public delegate void MqttClientChannelStateHandler(object sender, ChannelStateEventArgs args);
    public delegate void MqttClientChannelErrorHandler(object sender, ChannelErrorEventArgs args);
    
    
    public class PiraeusMqttClient
    {
        /// <summary>
        /// Creates a new instance of MQTT client.
        /// </summary>
        /// <param name="config">MQTT configuration.</param>
        /// <param name="channel">Channel for communications.</param>
        /// <param name="dispatcher">Optional custom dispatcher to receive published messages.</param>
        public PiraeusMqttClient(MqttConfig config, IChannel channel, IMqttDispatch dispatcher = null)
        {
            this.dispatcher = dispatcher != null ? dispatcher : new GenericMqttDispatcher();
            timeoutMilliseconds = config.MaxTransmitSpan.TotalMilliseconds;
            session = new MqttSession(config);
            session.OnKeepAlive += Session_OnKeepAlive;
            session.OnConnect += Session_OnConnect;
            session.OnDisconnect += Session_OnDisconnect;
            session.OnRetry += Session_OnRetry;

            this.channel = channel;
            this.channel.OnReceive += Channel_OnReceive;
            this.channel.OnClose += Channel_OnClose;
            this.channel.OnError += Channel_OnError;
            this.channel.OnStateChange += Channel_OnStateChange;

            queue = new Queue<byte[]>();
        }

        

        public event MqttClientChannelStateHandler OnChannelStateChange;
        public event MqttClientChannelErrorHandler OnChannelError;

        private IChannel channel;
        private MqttSession session;
        private ConnectAckCode? code;
        private double timeoutMilliseconds;
        private IMqttDispatch dispatcher;
        private Queue<byte[]> queue;


        public bool IsConnected
        {
            get { return channel.IsConnected; }
        }

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
                try
                {
                    await channel.OpenAsync();
                }
                catch(Exception ex)
                {
                    OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
                    return ConnectAckCode.ServerUnavailable;
                }

                try
                {
                    Receive(channel);
                }
                catch(Exception ex)
                {
                    OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
                    return ConnectAckCode.ServerUnavailable;
                }
            }

            try
            {
                await channel.SendAsync(msg.Encode());

                DateTime expiry = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
                while (!code.HasValue)
                {
                    await Task.Delay(10);
                    if (DateTime.UtcNow > expiry)
                    {
                        throw new TimeoutException("MQTT connection timed out.");
                    }
                }

                return code.Value;
            }
            catch(Exception ex)
            {
                OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
                return ConnectAckCode.ServerUnavailable;
            }
        }

        private void Receive(IChannel channel)
        {
            try
            {
                Task task = channel.ReceiveAsync();
                Task.WhenAll(task);
            }
            catch(AggregateException ae)
            {
                Console.WriteLine("Receive AggregateException '{0}'", ae.Flatten().InnerException.Message);
            }
        }

        public async Task CloseAsync()
        {
            if(session != null)
            {
                session.Dispose();
                session = null;
            }

            try
            {
                channel.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Channel close exception {0}", ex.Message);
                Console.WriteLine("Channel close exception stack trace {0}", ex.StackTrace);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Disconnect from Piraeus
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            string id = null;

            try
            {
                id = channel.Id;
                DisconnectMessage msg = new DisconnectMessage();

                if (channel.IsConnected)
                {
                    await channel.SendAsync(msg.Encode());
                }                
            }
            catch(Exception ex)
            {
                string disconnectMsgError = String.Format("ERROR: Sending MQTT Disconnect message '{0}'", ex.Message);
                Console.WriteLine(disconnectMsgError);
                Trace.TraceError(disconnectMsgError);
            }

            try
            {
                channel.Dispose();
            }
            catch(Exception ex)
            {
                string channelDisposeMsgError = String.Format("ERROR: MQTT channel dispose after disconnect '{0}'", ex.Message);
                Console.WriteLine(channelDisposeMsgError);
                Trace.TraceError(channelDisposeMsgError);
            }

            try
            {
                if(session != null)
                {
                    session.Dispose();
                    session = null;
                }
            }
            catch(Exception ex)
            {
                string sessionDisposeMsgError = String.Format("ERROR: MQTT session dispose after disconnect '{0}'", ex.Message);
                Console.WriteLine(sessionDisposeMsgError);
                Trace.TraceError(sessionDisposeMsgError);
            }
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
        public async Task PublishAsync(QualityOfServiceLevelType qos, string topicUriString, string contentType, byte[] data, string cacheKey = null, IEnumerable<KeyValuePair<string, string>> indexes = null, string messageId = null)
        {
            try
            {

                string indexString = GetIndexString(indexes);

                UriBuilder builder = new UriBuilder(topicUriString);
                string queryString = messageId == null ? String.Format("{0}={1}", SkunkLab.Protocols.Utilities.QueryStringConstants.CONTENT_TYPE, contentType) : String.Format("{0}={1}&{2}={3}", QueryStringConstants.CONTENT_TYPE, contentType, QueryStringConstants.MESSAGE_ID, messageId);

                if (!string.IsNullOrEmpty(cacheKey))
                {
                    queryString = queryString + String.Format("&{0}={1}", QueryStringConstants.CACHE_KEY, cacheKey);
                }

                if (!string.IsNullOrEmpty(indexString))
                {
                    queryString = queryString + "&" + indexString;
                }


                builder.Query = queryString;

                PublishMessage msg = new PublishMessage(false, qos, false, 0, builder.ToString(), data);
                if (qos != QualityOfServiceLevelType.AtMostOnce)
                {
                    msg.MessageId = session.NewId();
                    session.Quarantine(msg, DirectionType.Out);
                }

                queue.Enqueue(msg.Encode());

                while (queue.Count > 0)
                {
                    byte[] message = queue.Dequeue();

                    if (channel.RequireBlocking)
                    {
                        Task t = channel.SendAsync(message);
                        Task.WaitAll(t);
                    }
                    else
                    {
                        await channel.SendAsync(message);
                    }
                }
            }
            catch(Exception ex)
            {
                OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
            }

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
            try
            {
                Dictionary<string, QualityOfServiceLevelType> dict = new Dictionary<string, QualityOfServiceLevelType>();
                dict.Add(topicUriString, qos);
                dispatcher.Register(topicUriString, action);
                SubscribeMessage msg = new SubscribeMessage(session.NewId(), dict);


                if (channel.RequireBlocking)
                {
                    Task t = channel.SendAsync(msg.Encode());
                    Task.WaitAll(t);
                }
                else
                {
                    await channel.SendAsync(msg.Encode());
                }
            }
            catch(Exception ex)
            {
                OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
            }
        }

        /// <summary>
        /// MQTT subscribe to a topic.  Remember, these subscriptions are ephemeral.
        /// </summary>
        /// <param name="subscriptions"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(Tuple<string, QualityOfServiceLevelType, Action<string, string, byte[]>>[] subscriptions)
        {
            try
            {
                Dictionary<string, QualityOfServiceLevelType> dict = new Dictionary<string, QualityOfServiceLevelType>();

                foreach (var tuple in subscriptions)
                {
                    dict.Add(tuple.Item1, tuple.Item2);
                    dispatcher.Register(tuple.Item1, tuple.Item3);
                }

                SubscribeMessage msg = new SubscribeMessage(session.NewId(), dict);

                if (channel.RequireBlocking)
                {
                    Task t = channel.SendAsync(msg.Encode());
                    Task.WaitAll(t);
                }
                else
                {
                    await channel.SendAsync(msg.Encode());
                }
            }
            catch(Exception ex)
            {
                OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
            }
        }

        /// <summary>
        /// Unsubscribe from an ephemeral subscription.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(string topic)
        {
            try
            {
                UnsubscribeMessage msg = new UnsubscribeMessage(session.NewId(), new string[] { topic });

                if (channel.RequireBlocking)
                {
                    Task t = channel.SendAsync(msg.Encode());
                    Task.WaitAll(t);
                }
                else
                {
                    await channel.SendAsync(msg.Encode());
                }

                dispatcher.Unregister(topic);
            }
            catch(Exception ex)
            {
                OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
            }
        }

        /// <summary>
        /// Unsubscribe from an ephemeral subscription.
        /// </summary>
        /// <param name="topics"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(IEnumerable<string> topics)
        {
            try
            {
                UnsubscribeMessage msg = new UnsubscribeMessage(session.NewId(), topics);

                if (channel.RequireBlocking)
                {
                    Task t = channel.SendAsync(msg.Encode());
                    Task.WaitAll(t);
                }
                else
                {
                    await channel.SendAsync(msg.Encode());
                }

                foreach (var topic in topics)
                {
                    dispatcher.Unregister(topic);
                }
            }
            catch(Exception ex)
            {
                OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, ex));
            }
        }

        #region Channel Events
        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs args)
        {
            MqttMessage msg = MqttMessage.DecodeMessage(args.Message);
            MqttMessageHandler handler = MqttMessageHandler.Create(session, msg, dispatcher);

            Task task = Task.Factory.StartNew(async () =>
            {
                try
                {
                    MqttMessage message = await handler.ProcessAsync();
                    if (message != null)
                    {
                        await channel.SendAsync(message.Encode());
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Trace.TraceError(ex.Message);
                }
            });           

            Task.WaitAll(task);

            if(task.Exception != null)
            {
                OnChannelError?.Invoke(this, new ChannelErrorEventArgs(channel.Id, task.Exception.InnerException));
            }
        }


        private string GetIndexString(IEnumerable<KeyValuePair<string, string>> indexes = null)
        {
            if(indexes == null)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            foreach(KeyValuePair<string,string> kvp in indexes)
            {
                if(builder.ToString().Length == 0)
                {
                    builder.Append(String.Format("i={0};{1}", kvp.Key, kvp.Value));
                }
                else
                {
                    builder.Append(String.Format("&i={0};{1}", kvp.Key, kvp.Value));
                }
            }

            return builder.ToString();


        }
       

        private void Channel_OnError(object sender, ChannelErrorEventArgs args)
        {
            OnChannelError?.Invoke(this, args);
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs args)
        {
            try
            {
                code = null;
                this.channel.Dispose();
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Piraeus MQTT client fault disposing channel.");
                Trace.TraceError(ex.Message);
            }
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

       
        private void Session_OnKeepAlive(object sender, MqttMessageEventArgs args)
        {
            try
            {               
                Task task = channel.SendAsync(args.Message.Encode());
                if (channel.RequireBlocking)
                {

                    Task.WaitAll(task);
                }
                else
                {
                    Task.WhenAll(task);
                }

            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
                Console.ResetColor();
            }
        }

        #endregion
    }
}
