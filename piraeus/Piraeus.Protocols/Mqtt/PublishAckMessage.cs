

namespace Piraeus.Protocols.Mqtt
{
    using System;
    public class PublishAckMessage : MqttMessage
    {
        public PublishAckMessage()
        {
        }

        public PublishAckMessage(PublishAckType ackType)
        {
            this.AckType = ackType;
        }

        public override bool HasAck
        {
            get { return (this.AckType == PublishAckType.PUBREC || this.AckType == PublishAckType.PUBREL); }
        }

        public PublishAckType AckType { get; set; }

        public ushort MessageId { get; set; }

        public override byte[] Encode()
        {
            byte ackType = Convert.ToByte((int)this.AckType);
            byte fixedHeader = (byte)((ackType << Constants.Header.MessageTypeOffset) |
                    (byte)(0x00) |
                    (byte)(0x00) |
                    (byte)(0x00));
            
            byte[] remainingLength = base.EncodeRemainingLength(2);

            byte[] buffer = new byte[4];
            buffer[0] = fixedHeader;
            buffer[1] = remainingLength[0];
            buffer[2] = (byte)((this.MessageId >> 8) & 0x00FF); //MSB
            buffer[3] = (byte)(this.MessageId & 0x00FF); //LSB

            return buffer;
        }

        internal override MqttMessage Decode(byte[] message)
        {
            MqttMessage pubackMessage = new PublishAckMessage();

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

            return pubackMessage; 
        }
    }
}
