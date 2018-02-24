

namespace Piraeus.Protocols.Mqtt
{
    using System;
    public class PublishMessage : MqttMessage
    {
        public PublishMessage()
        {
        }


        public PublishMessage(bool dupFlag, QualityOfServiceLevelType qosLevel, bool retainFlag, ushort messageId, string topic, byte[] data)
        {
            this.DupFlag = dupFlag;
            this.QualityOfServiceLevel = qosLevel;
            this.RetainFlag = retainFlag;

            this.MessageId = messageId;
            this.Topic = topic;
            this.Payload = data;
        }

        public override MqttMessageType MessageType
        {
            get
            {
                return MqttMessageType.PUBLISH;
            }
            internal set
            {
                base.MessageType = value;
            }
        }

        public override bool HasAck
        {
            get { return this.QualityOfServiceLevel != QualityOfServiceLevelType.AtMostOnce; }
        }

        public bool DupFlag
        {
            get { return base.Dup; }
            set { base.Dup = value; }
        }

        public QualityOfServiceLevelType QualityOfServiceLevel
        {
            get { return base.QualityOfService; }
            set { base.QualityOfService = value; }
        }

        public bool RetainFlag
        {
            get { return base.Retain; }
            set { base.Retain = value; }
        }

        public ushort MessageId { get; set; }
        public string Topic { get; set; }
        //public byte[] Data { get; set; }

        public override byte[] Encode()
        {
            byte qos = (byte)(int)this.QualityOfServiceLevel;

            byte fixedHeader = (byte)((0x03 << Constants.Header.MessageTypeOffset) |
                   (byte)(qos << Constants.Header.QosLevelOffset) |
                   (byte)(this.Dup ? (byte)(0x01 << Constants.Header.DupFlagOffset) : (byte)0x00) |
                   (byte)(this.Retain ? (byte)(0x01) : (byte)0x00));

            //PublishVariableHeader variableHeader = this.VariableHeader as PublishVariableHeader;
            //PublishPayload payload = this.Payload as PublishPayload;

            ByteContainer vhContainer = new ByteContainer();
            vhContainer.Add(this.Topic);

            byte[] messageId = new byte[2];
            messageId[0] = (byte)((this.MessageId >> 8) & 0x00FF); // MSB
            messageId[1] = (byte)(this.MessageId & 0x00FF); // LSB

            vhContainer.Add(messageId);

            byte[] variableHeaderBytes = vhContainer.ToBytes();


            //byte[] variableHeaderBytes = variableHeader.Encode();
            //byte[] payloadBytes = payload.Encode();

            byte[] lengthSB = new byte[2];
            lengthSB[0] = (byte)((this.Payload.Length >> 8) & 0x00FF); // MSB
            lengthSB[1] = (byte)(this.Payload.Length & 0x00FF); // LSB

            ByteContainer payloadContainer = new ByteContainer();
            payloadContainer.Add(lengthSB);
            payloadContainer.Add(this.Payload);

            byte[] payloadBytes = payloadContainer.ToBytes();


            int remainingLength = variableHeaderBytes.Length + payloadBytes.Length;
            byte[] remainingLengthBytes = base.EncodeRemainingLength(remainingLength);

            ByteContainer container = new ByteContainer();
            container.Add(fixedHeader);
            container.Add(remainingLengthBytes);
            container.Add(variableHeaderBytes);
            container.Add(payloadBytes);

            return container.ToBytes();
        }

        internal override MqttMessage Decode(byte[] message)
        {
            MqttMessage publishMessage = new PublishMessage();

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

            //base.VariableHeader = new PublishVariableHeader();

            index = 0;
            int length = 0;
            this.Topic = ByteContainer.DecodeString(buffer, index, out length);
            index += length;

            ushort messageId = (ushort)((buffer[index++] << 8) & 0xFF00);
            messageId |= buffer[index++];

            this.MessageId = messageId;

            length = ((buffer[index++] << 8) & 0xFF00);
            length |= buffer[index++];

            byte[] data = new byte[length];
            Buffer.BlockCopy(buffer, index, data, 0, length);

            this.Payload = data;
            //index = base.VariableHeader.Decode(buffer);
            
            //base.Payload = new PublishPayload();
            //base.Payload.Decode(buffer, base.VariableHeader, index);

            return publishMessage;
        }
    }
}
