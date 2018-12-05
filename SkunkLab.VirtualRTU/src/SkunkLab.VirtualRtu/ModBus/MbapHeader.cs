using System;

namespace SkunkLab.VirtualRtu.ModBus
{
    public class MbapHeader
    {
        public ushort TransactionId { get; set; }
        public ushort ProtocolId { get; set; }
        public ushort Length { get; set; }
        public byte UnitId { get; set; }


        public static MbapHeader Decode(byte[] message)
        {
            if (message.Length < 7)
            {
                return null;
            }

            MbapHeader header = new MbapHeader();

            int index = 0;

            header.TransactionId = (ushort)(message[index++] << 0x08 | message[index++]);
            header.ProtocolId = (ushort)(message[index++] << 0x08 | message[index++]);
            header.Length = (ushort)(message[index++] << 0x08 | message[index++]);
            header.UnitId = Convert.ToByte(message[index]);

            return header;
        }

        public byte[] Encode()
        {
            int index = 0;
            byte[] header = new byte[7];
            header[index++] = (byte)((TransactionId >> 8) & 0x00FF); //MSB
            header[index++] = (byte)(TransactionId & 0x00FF); //LSB
            header[index++] = (byte)((ProtocolId >> 8) & 0x00FF); //MSB
            header[index++] = (byte)(ProtocolId & 0x00FF); //LSB
            header[index++] = (byte)((Length >> 8) & 0x00FF); //MSB
            header[index++] = (byte)(Length & 0x00FF); //LSB
            header[index++] = UnitId;

            return header;
        }
    }
}
