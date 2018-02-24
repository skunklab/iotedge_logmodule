
namespace Piraeus.Protocols.Mqtt
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class ByteContainer
    {
        public ByteContainer()
        {
            header = new List<byte>();
        }

        private List<byte> header;


        public static string DecodeString(byte[] buffer, int index, out int length)
        {
            length = ((buffer[index++] << 8) & 0xFF00);
            length |= buffer[index++];

            byte[] encodedBytes = new byte[length];
            Buffer.BlockCopy(buffer, index, encodedBytes, 0, length);

            length += 2;
            return Encoding.UTF8.GetString(encodedBytes);
        }
        
        public void Add(byte value)
        {
            header.Add(value);
        }

        public int Length
        {
            get { return header.Count; }
        }

        public void Add(byte[] array)
        {
            int index = 0;
            while(index < array.Length)
            {
                header.Add(array[index]);
                index++;
            }
        }

        public void Add(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            byte[] valueArray = null;

            valueArray = Encoding.UTF8.GetBytes(value);
            
            byte[] buffer = new byte[valueArray.Length + 2];

            byte[] lengthSB = new byte[2];
            lengthSB[0] = (byte)((value.Length >> 8) & 0x00FF); // MSB
            lengthSB[1] = (byte)(value.Length & 0x00FF); // LSB

            Buffer.BlockCopy(lengthSB, 0, buffer, 0, lengthSB.Length);
            Buffer.BlockCopy(valueArray, 0, buffer, 2, valueArray.Length);

            Add(buffer);
        }
        
        public byte[] ToBytes()
        {
            return header.ToArray();
        }
    }
}
