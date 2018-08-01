using System;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt.Handlers
{
    public abstract class MqttMessageHandler
    {
        public static MqttMessageHandler Create(MqttSession session, MqttMessage message, IMqttDispatch dispatcher = null)
        {
            
            switch(message.MessageType)
            {
                case MqttMessageType.CONNACK:
                    return new MqttConnackHandler(session, message);
                case MqttMessageType.CONNECT:
                    return new MqttConnectHandler(session, message);
                case MqttMessageType.DISCONNECT:
                    return new MqttDisconnectHandler(session, message);
                case MqttMessageType.PINGREQ:
                    return new MqttPingReqHandler(session, message);
                case MqttMessageType.PINGRESP:
                    return new MqttPingRespHandler(session,  message);
                case MqttMessageType.PUBACK:
                    return new MqttPubAckHandler(session, message);
                case MqttMessageType.PUBCOMP:
                    return new MqttPubCompHandler(session, message);
                case MqttMessageType.PUBLISH:
                    return new MqttPublishHandler(session, message, dispatcher);
                case MqttMessageType.PUBREC:
                    return new MqttPubRecHandler(session, message);
                case MqttMessageType.PUBREL:
                    return new MqttPubRelHandler(session, message, dispatcher);
                case MqttMessageType.SUBACK:
                    return new MqttSubAckHandler(session, message);
                case MqttMessageType.SUBSCRIBE:
                    return new MqttSubscribeHandler(session, message);
                case MqttMessageType.UNSUBACK:
                    return new MqttUnsubAckHandler(session, message);
                case MqttMessageType.UNSUBSCRIBE:
                    return new MqttUnsubscribeHandler(session, message);
                default:
                    throw new InvalidCastException("MqttMessageType");
            }
            
        }

        protected MqttMessageHandler(MqttSession session, MqttMessage message, IMqttDispatch dispatcher = null)
        {
            Session = session;            
            Message = message;
            Dispatcher = dispatcher;
        }

        protected IMqttDispatch Dispatcher { get; set; }

        protected MqttSession Session { get; set; }

        protected MqttMessage Message { get; set; }

        public abstract Task<MqttMessage> ProcessAsync();
    }
}
