

namespace SkunkLab.Protocols.Mqtt
{
    using System;
    using System.Collections.Generic;
    public abstract class MqttMessage
    {
        #region Fixed Header
        public virtual MqttMessageType MessageType { get; internal set; }

        public bool Dup { get; set; }

        public QualityOfServiceLevelType QualityOfService { get; set; }

        protected bool Retain { get; set; }

        public byte[] Payload { get; set; }

        #endregion

        public abstract bool HasAck { get; }

        public virtual ushort MessageId { get; set; }

        public abstract byte[] Encode();

        internal abstract MqttMessage Decode(byte[] message);

        public static MqttMessage DecodeMessage(byte[] message)
        {
            byte fixedHeader = message[0];
            byte msgType = (byte)(fixedHeader >> 0x04);

            MqttMessageType messageType = (MqttMessageType)msgType;
            
            MqttMessage mqttMessage = null;

            switch (messageType)
            {
                case MqttMessageType.CONNECT:
                    mqttMessage = new ConnectMessage();
                    break;

                case MqttMessageType.CONNACK:
                    mqttMessage = new ConnectAckMessage();
                    break;
                case MqttMessageType.PUBLISH:
                    mqttMessage = new PublishMessage();
                    break;

                case MqttMessageType.PUBACK:
                    mqttMessage = new PublishAckMessage();
                    break;

                case MqttMessageType.PUBREC:
                    mqttMessage = new PublishAckMessage();
                    break;

                case MqttMessageType.PUBREL:
                    mqttMessage = new PublishAckMessage();
                    break;

                case MqttMessageType.PUBCOMP:
                    mqttMessage = new PublishAckMessage();
                    break;

                case MqttMessageType.SUBSCRIBE:
                    mqttMessage = new SubscribeMessage();
                    break;

                case MqttMessageType.SUBACK:
                    mqttMessage = new SubscriptionAckMessage();
                    break;

                case MqttMessageType.UNSUBSCRIBE:
                    mqttMessage = new UnsubscribeMessage();
                    break;

                case MqttMessageType.UNSUBACK:
                    mqttMessage = new UnsubscribeAckMessage();
                    break;

                case MqttMessageType.PINGREQ:
                    mqttMessage = new PingRequestMessage();
                    break;

                case MqttMessageType.PINGRESP:
                    mqttMessage = new PingResponseMessage();
                    break;

                case MqttMessageType.DISCONNECT:
                    mqttMessage = new DisconnectMessage();
                    break;

                default:
                    throw new InvalidOperationException("Unknown message type.");
            }

            mqttMessage.Decode(message);
            return mqttMessage;
        }

        internal byte[] EncodeRemainingLength(int remainingLength)
        {
            //do digit = X MOD 128 X = X DIV 128 
            // if there are more digits to encode, set the top bit of this digit if ( X > 0 ) digit = digit OR 0x80 endif 'output' digit while ( X> 0 )
            List<byte> list = new List<byte>();
            int digit = 0;
            do
            {
                digit = remainingLength % 128;
                remainingLength /= 128;

                if (remainingLength > 0)
                {
                    digit = digit | 0x80;
                }

                list.Add((byte)digit);

            } while (remainingLength > 0);

            if (list.Count > 4)
            {
                throw new InvalidOperationException("Invalid remaining length;");
            }

            return list.ToArray();
        }

        internal void DecodeFixedHeader(byte fixedHeader)
        {
            byte msgType = (byte)(fixedHeader >> 0x04);
            byte qosLevel = (byte)((fixedHeader & 0x06) >> 0x01);
            byte dupFlag = (byte)((fixedHeader & 0x08) >> 0x03);
            byte retainFlag = (byte)((fixedHeader & 0x01));
            
            this.MessageType = (MqttMessageType)(int)msgType;
            this.QualityOfService = (QualityOfServiceLevelType)(int)qosLevel;
            this.Dup = dupFlag == 0 ? false : true;
            this.Retain = retainFlag == 0 ? false : true;

        }

        internal int DecodeRemainingLength(byte[] buffer)
        {
            int index = 0;
            int multiplier = 1;
            int value = 0;
            int digit = 0;
            index++;
            byte[] nextByte = new byte[1];
            do
            {
                digit = buffer[index++];
                value += ((digit & 127) * multiplier);
                multiplier *= 128;
            } while ((digit & 128) != 0);
            return value;
        }

        

    }
}
