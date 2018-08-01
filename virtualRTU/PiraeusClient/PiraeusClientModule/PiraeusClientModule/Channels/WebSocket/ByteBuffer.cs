
namespace PiraeusClientModule.Channels.WebSocket
{
    
    
        using System;
        using System.Collections.Generic;
        using System.Text;

        internal sealed class ByteBuffer
        {
            private int _currentLength;
            private readonly int _maxLength;
            private readonly List<byte[]> _segments = new List<byte[]>();

            public ByteBuffer(int maxLength)
            {
                this._maxLength = maxLength;
            }

            public void Append(byte[] segment)
            {
                this._currentLength += segment.Length;
                if (this._currentLength > this._maxLength)
                {
                    throw new InvalidOperationException("Length exceeded.");
                }
                this._segments.Add(segment);
            }

            public byte[] GetByteArray()
            {
                byte[] dst = new byte[this._currentLength];
                int dstOffset = 0;
                for (int i = 0; i < this._segments.Count; i++)
                {
                    byte[] src = this._segments[i];
                    Buffer.BlockCopy(src, 0, dst, dstOffset, src.Length);
                    dstOffset += src.Length;
                }
                return dst;
            }

            public string GetString()
            {
                StringBuilder builder = new StringBuilder();
                System.Text.Decoder decoder = Encoding.UTF8.GetDecoder();
                for (int i = 0; i < this._segments.Count; i++)
                {
                    bool flush = i == (this._segments.Count - 1);
                    byte[] bytes = this._segments[i];
                    char[] chars = new char[decoder.GetCharCount(bytes, 0, bytes.Length, flush)];
                    int charCount = decoder.GetChars(bytes, 0, bytes.Length, chars, 0, flush);
                    builder.Append(chars, 0, charCount);
                }
                return builder.ToString();
            }
        }
    

}
