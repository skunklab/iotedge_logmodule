

namespace Piraeus.Protocols.Mqtt
{
    using System;
    using System.Collections.Generic;
    public class SubscriptionAckMessage : MqttMessage
    {
        public SubscriptionAckMessage()
        {
            this._qosLevels = new QualityOfServiceLevelCollection();
        }

        public SubscriptionAckMessage(ushort messageId, IEnumerable<QualityOfServiceLevelType> qosLevels)
        {
            this.MessageId = messageId;
            this._qosLevels = new QualityOfServiceLevelCollection(qosLevels);
        }

        private QualityOfServiceLevelCollection _qosLevels;

        public ushort MessageId { get; set; }

        public override bool HasAck
        {
            get { return false; }
        }

        public override MqttMessageType MessageType
        {
            get
            {
                return MqttMessageType.SUBACK;
            }

            internal set
            {
                base.MessageType = value;
            }
        }

        public QualityOfServiceLevelCollection QualityOfServiceLevels
        {
            get { return this._qosLevels; }
        }

        public override byte[] Encode()
        {
            byte fixedHeader = (byte)((0x09 << Constants.Header.MessageTypeOffset) |
                   (byte)(0x00) |
                   (byte)(0x00) |
                   (byte)(0x00));

            byte[] messageId = new byte[2];
            messageId[0] = (byte)((this.MessageId >> 8) & 0x00FF); //MSB
            messageId[1] = (byte)(this.MessageId & 0x00FF); //LSB

            ByteContainer qosContainer = new ByteContainer();
            int index = 0;
            while(index < this._qosLevels.Count)
            {
                byte qos = (byte)(int)this._qosLevels[index];
                qosContainer.Add(qos);
                index++;
            }

            byte[] payload = qosContainer.ToBytes();

            byte[] remainingLengthBytes = EncodeRemainingLength(2 + payload.Length);

            ByteContainer container = new ByteContainer();
            container.Add(fixedHeader);
            container.Add(remainingLengthBytes);
            container.Add(messageId);
            container.Add(payload);

            return container.ToBytes();
        }

        internal override MqttMessage Decode(byte[] message)
        {
            SubscriptionAckMessage subackMessage = new SubscriptionAckMessage();

            int index = 0;
            byte fixedHeader = message[index];
            base.DecodeFixedHeader(fixedHeader);

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

            this.MessageId = messageId;

            while (index < buffer.Length)
            {
                QualityOfServiceLevelType qosLevel = (QualityOfServiceLevelType)buffer[index++];
                this._qosLevels.Add(qosLevel);
            }


            return subackMessage; 
        }
    }
}
