using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using SkunkLab.Protocols.Mqtt.Handlers;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Mqtt
{
    public delegate void ConnectionHandler(object sender, MqttConnectionArgs args);
    public delegate void EventHandler<MqttMessageEventArgs>(object sender, MqttMessageEventArgs args);
    public class MqttSession
    {
       
        public MqttSession(MqttConfig config)
        {
            this.config = config;            
            pubContainer = new PublishContainer(config);
            qosLevels = new Dictionary<string, QualityOfServiceLevelType>();
            quarantine = new MqttQuarantineTimer(config);
            quarantine.OnRetry += Quarantine_OnRetry;
        }

        

        public event ConnectionHandler OnConnect;                       //client function
        public event EventHandler<MqttMessageEventArgs> OnRetry;        //client function               
        public event EventHandler<MqttMessageEventArgs> OnSubscribe;    //server function
        public event EventHandler<MqttMessageEventArgs> OnUnsubscribe;  //server function
        public event EventHandler<MqttMessageEventArgs> OnPublish;      //server function
        public event EventHandler<MqttMessageEventArgs> OnDisconnect;   //client & server function
        public event EventHandler<MqttMessageEventArgs> OnKeepAlive;    //client function
        public event EventHandler<MqttMessageEventArgs> OnKeepAliveExpiry; //server function
        
        private MqttQuarantineTimer quarantine;     //quarantines ids for reuse and supplies valid ids
        private PublishContainer pubContainer;      //manages QoS 2 message features
        private MqttConfig config;                  //configuration
        private Timer keepaliveTimer;               //timer for tracking keepalives
        private int _keepaliveSeconds;              //keepalive time increment
        private DateTime keepaliveExpiry;           //expiry of the keepalive
        private Dictionary<string, QualityOfServiceLevelType> qosLevels;    //qos levels return from subscriptions
        private ConnectAckCode _code;

        public bool IsConnected { get; internal set; }
        public bool IsAuthenticated { get; internal set; }
        public ConnectAckCode ConnectResult
        {
            get { return _code; }
            internal set
            {
                OnConnect?.Invoke(this, new MqttConnectionArgs(value));
                _code = value;
            }
        }

        /// <summary>
        /// Returns a fresh usable message id.
        /// </summary>
        /// <returns></returns>
        public ushort NewId()
        {
            return quarantine.NewId();
        }

        /// <summary>
        /// Processes a receive MQTT message and a response or null (no response).
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<MqttMessage> ReceiveAsync(MqttMessage message)
        {
            MqttMessageHandler handler = MqttMessageHandler.Create(this, message);
            return await handler.ProcessAsync();
        }

        #region QoS Management
        public void AddQosLevel(string topic, QualityOfServiceLevelType qos)
        {
            if (!qosLevels.ContainsKey(topic))
            {
                qosLevels.Add(topic, qos);
            }
        }

        public QualityOfServiceLevelType? GetQoS(string topic)
        {
            if(qosLevels.ContainsKey(topic))
            {
                return qosLevels[topic];
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region internal function calls from handlers
        internal void Publish(MqttMessage message)
        {
            OnPublish?.Invoke(this, new MqttMessageEventArgs(message));
        }

        internal void Subscribe(MqttMessage message)
        {
            OnSubscribe?.Invoke(this, new MqttMessageEventArgs(message));
        }

        internal void Unsubscribe(MqttMessage message)
        {
            OnUnsubscribe?.Invoke(this, new MqttMessageEventArgs(message));
        }

        internal void Connect(ConnectAckCode code)
        {
            OnConnect?.Invoke(this, new MqttConnectionArgs(code));
        }

        internal void Disconnect(MqttMessage message)
        {
            OnDisconnect?.Invoke(this, new MqttMessageEventArgs(message));
        }

        #endregion

        #region QoS 2 functions
        internal void HoldMessage(MqttMessage message)
        {
            if (!pubContainer.ContainsKey(message.MessageId))
            {
                pubContainer.Add(message.MessageId, message);
            }
        }

        internal MqttMessage GetHeldMessage(ushort id)
        {
            if (pubContainer.ContainsKey(id))
            {
                return pubContainer[id];
            }
            else
            {
                return null;
            }
        }

        internal void ReleaseMessage(ushort id)
        {
            pubContainer.Remove(id);
        }

        #endregion

        #region keep alive

        internal int KeepAliveSeconds
        {
            get { return _keepaliveSeconds; }
            set
            {
                if (keepaliveTimer == null)
                {
                    keepaliveTimer = new Timer(Convert.ToDouble(value * 1000));
                    keepaliveTimer.Elapsed += KeepaliveTimer_Elapsed;
                    keepaliveTimer.Start();
                }
            }
        }
        internal void IncrementKeepAlive()
        {
            keepaliveExpiry = DateTime.UtcNow.AddSeconds(Convert.ToDouble(_keepaliveSeconds));
        }

        internal void StopKeepAlive()
        {
            keepaliveTimer.Stop();
            keepaliveTimer = null;
        }

        private void KeepaliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //communicates to server keep alive expired
            if (keepaliveExpiry.AddSeconds(Convert.ToDouble(_keepaliveSeconds) * 1.5) < DateTime.UtcNow)
            {
                OnKeepAliveExpiry?.Invoke(this, null);
                return;
            }

            //signals client to send a ping to keep alive
            if(keepaliveExpiry > DateTime.UtcNow)
            {                
                OnKeepAlive(this, new MqttMessageEventArgs(new PingRequestMessage()));
            }
        }

        #endregion

        #region ID Quarantine

        public bool IsQuarantined(ushort messageId)
        {
            return quarantine.ContainsKey(messageId);
        }
        public void Quarantine(MqttMessage message)
        {
            quarantine.Add(message);
        }

        public void Unquarantine(ushort messageId)
        {
            quarantine.Remove(messageId);
        }

        #endregion

        #region Retry Signal
        /// <summary>
        /// Signals a retry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Quarantine_OnRetry(object sender, MqttMessageEventArgs args)
        {
            MqttMessage msg = args.Message;
            msg.Dup = true;
            OnRetry?.Invoke(this, new MqttMessageEventArgs(msg));
        }

        #endregion
    }
}
