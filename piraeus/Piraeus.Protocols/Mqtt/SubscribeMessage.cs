

namespace Piraeus.Protocols.Mqtt
{
    using System;
    using System.Collections.Generic;
    public class SubscribeMessage : MqttMessage
    {
        public SubscribeMessage()
        {
            this._topics = new Dictionary<string, QualityOfServiceLevelType>();
        }

        public SubscribeMessage(ushort messageId, IDictionary<string, QualityOfServiceLevelType> topics)
        {
            this.MessageId = messageId;
            this._topics = topics;
        }



        private IDictionary<string, QualityOfServiceLevelType> _topics;
        public IDictionary<string, QualityOfServiceLevelType> Topics
        {
            get { return this._topics; }
        }

        public bool DupFlag
        {
            get { return base.Dup; }
            set { base.Dup = value; }
        }

        public override bool HasAck
        {
            get { return true; }
        }

        public override MqttMessageType MessageType
        {
            get
            {
                return MqttMessageType.SUBSCRIBE;
            }

            internal set
            {
                base.MessageType = value;
            }
        }

        public void AddTopic(string topic, QualityOfServiceLevelType qosLevel)
        {
            this._topics.Add(topic, qosLevel);
        }
        public void RemoveTopic(string topic)
        {
            if(this._topics.ContainsKey(topic))
            {
                this._topics.Remove(topic);
            }
        }

        public ushort MessageId { get; set; }

        public override byte[] Encode()
        {
            byte qos = Convert.ToByte((int)Enum.Parse(typeof(QualityOfServiceLevelType), this.QualityOfService.ToString(), false));

            byte fixedHeader = (byte)((0x08 << Constants.Header.MessageTypeOffset) |
                   (byte)(qos << Constants.Header.QosLevelOffset) |
                   (byte)(this.Dup ? (byte)(0x01 << Constants.Header.DupFlagOffset) : (byte)0x00) |
                   (byte)(this.Retain ? (byte)(0x01) : (byte)0x00));

            byte[] messageId = new byte[2];
            messageId[0] = (byte)((this.MessageId >> 8) & 0x00FF); // MSB
            messageId[1] = (byte)(this.MessageId & 0x00FF); // LSB
            
            ByteContainer payloadContainer = new ByteContainer();

            IEnumerator<KeyValuePair<string, QualityOfServiceLevelType>> en = this._topics.GetEnumerator();
            while(en.MoveNext())
            {
                string topic = en.Current.Key;
                QualityOfServiceLevelType qosLevel = this._topics[topic];
                payloadContainer.Add(topic);
                byte topicQos = Convert.ToByte((int)qosLevel);
                payloadContainer.Add(topicQos);
            }

            //int index = 0;
            //while (index < this._topics.Count)
            //{
            //    string topic = this._topics[index].Item1;
            //    QualityOfServiceLevelType qosLevel = this._topics[index].Item2;
            //    payloadContainer.Add(topic);
            //    byte topicQos = Convert.ToByte((int)qosLevel);
            //    payloadContainer.Add(topicQos);
            //    index++;
            //}

            byte[] payload = payloadContainer.ToBytes();

            byte[] remainingLengthBytes = EncodeRemainingLength(payload.Length + 2);

            ByteContainer container = new ByteContainer();
            container.Add(fixedHeader);
            container.Add(remainingLengthBytes);
            container.Add(messageId);
            container.Add(payload);

            return container.ToBytes();
        }

        internal override MqttMessage Decode(byte[] message)
        {
            SubscribeMessage subscribeMessage = new SubscribeMessage();

            int index = 0;
            byte fixedHeader = message[index];
            subscribeMessage.DecodeFixedHeader(fixedHeader);

            int remainingLength = base.DecodeRemainingLength(message);

            int temp = remainingLength; //increase the fixed header size
            do
            {
                index++;
                temp = temp / 128;
            } while (temp > 0);

            index++;

            byte[] buffer = new byte[remainingLength];
            Buffer.BlockCopy(message, index, buffer, 0, buffer.Length);

            ushort messageId = (ushort)((buffer[0] << 8) & 0xFF00);
            messageId |= buffer[1];

            subscribeMessage.MessageId = messageId;

            while (index < buffer.Length)
            {
                int length = 0;
                string topic = ByteContainer.DecodeString(buffer, index, out length);
                index += length;
                QualityOfServiceLevelType topicQosLevel = (QualityOfServiceLevelType)buffer[index++];
                //subscribeMessage._topics.Add(new Tuple<string, QualityOfServiceLevelType>(topic, topicQosLevel));
                //subscribeMessage.Topics.Add(topic, topicQosLevel);
                this._topics.Add(topic, topicQosLevel);
            }

            return subscribeMessage;
        }
    }
}
