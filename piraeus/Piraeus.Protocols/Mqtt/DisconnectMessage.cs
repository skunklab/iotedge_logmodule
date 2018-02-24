
namespace Piraeus.Protocols.Mqtt
{
    public class DisconnectMessage : MqttMessage
    {

        public override bool HasAck
        {
            get { return false; }
        }
        public override byte[] Encode()
        {
            int index = 0;
            byte[] buffer = new byte[2];

            buffer[index++] = (byte)((0x0E << Constants.Header.MessageTypeOffset) |
                   (byte)(0x00) |
                   (byte)(0x00) |
                   (byte)(0x00));

            buffer[index] = 0x00;

            return buffer;
        }

        internal override MqttMessage Decode(byte[] message)
        {
            DisconnectMessage disconnect = new DisconnectMessage();
            int index = 0;
            byte fixedHeader = message[index];
            base.DecodeFixedHeader(fixedHeader);

            int remainingLength = base.DecodeRemainingLength(message);

            if (remainingLength != 0)
            {
                //fault
            }

            return disconnect;

        }
    }
}
