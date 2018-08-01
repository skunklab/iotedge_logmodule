
namespace SkunkLab.Protocols.Mqtt
{
    using System;

    public class UnsubscribeAckMessage : MqttMessage
    {
        public UnsubscribeAckMessage()
        {

        }

        public UnsubscribeAckMessage(ushort messageId)
        {
            this.MessageId = messageId;
        }

        //public ushort MessageId { get; set; }

        public override bool HasAck
        {
            get { return false; }
        }

        public override byte[] Encode()
        {
            byte fixedHeader = (byte)((0x0B << Constants.Header.MessageTypeOffset) |
                   (byte)(0x00) |
                   (byte)(0x00) |
                   (byte)(0x00));

            byte[] messageId = new byte[2];
            messageId[0] = (byte)((this.MessageId >> 8) & 0x00FF); //MSB
            messageId[1] = (byte)(this.MessageId & 0x00FF); //LSB



            byte[] remainingLengthBytes = base.EncodeRemainingLength(2);


            ByteContainer container = new ByteContainer();
            container.Add(fixedHeader);
            container.Add(remainingLengthBytes);
            container.Add(messageId);

            return container.ToBytes();
        }

        internal override MqttMessage Decode(byte[] message)
        {
            UnsubscribeAckMessage unsubackMessage = new UnsubscribeAckMessage();

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


            return unsubackMessage; 
        }
    }
}
