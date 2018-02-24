
namespace Piraeus.Protocols.Mqtt
{
    using System;

    public class ConnectAckMessage : MqttMessage
    {
        public ConnectAckMessage()
        {

        }
        public ConnectAckMessage(bool sessionPresent, ConnectAckCode returnCode)
        {
            this.SessionPresent = sessionPresent;
            this.ReturnCode = returnCode;
        }

        public override bool HasAck
        {
            get { return false; }
        }

        public bool SessionPresent { get; set; }

        public ConnectAckCode ReturnCode { get; set; }

        public override byte[] Encode()
        {
            int index = 0;
            byte[] buffer = new byte[4];

            buffer[index++] = (byte)((0x02 << Constants.Header.MessageTypeOffset) |
                   (byte)(0x00) |
                   (byte)(0x00) |
                   (byte)(0x00));

          
            
            buffer[index++] = (byte)0x02; //2 remaining bytes

            //byte[] remainingLengthBytes = base.EncodeRemainingLength(2);
            buffer[index++] = this.SessionPresent ? (byte)0x01 : (byte)0x00;
            buffer[index++] = (byte)(int)this.ReturnCode;

            //Buffer.BlockCopy(remainingLengthBytes, 0, buffer, 2, remainingLengthBytes.Length);

           
            return buffer;
        }

        internal override MqttMessage Decode(byte[] message)
        {
            ConnectAckMessage connackMessage = new ConnectAckMessage();

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

            //base.VariableHeader = new ConnackVariableHeader();

            //index = base.VariableHeader.Decode(buffer);

            index = 0;
            byte reserved = buffer[index++];

            if (reserved != 0x00)
            {
                this.SessionPresent = Convert.ToBoolean(reserved);
            }

            byte code = buffer[index++];

            this.ReturnCode = (ConnectAckCode)code;
            

            return connackMessage;
        }
    }
}
