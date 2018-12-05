using System;

namespace LogModule
{
    public class BytesTransferredEventArgs : EventArgs
    {
        public BytesTransferredEventArgs(string filename, long length, long bytesTransferred)
        {
            Filename = filename;
            BytesTransferred = bytesTransferred;
            Length = length;
        }

        public string Filename { get; internal set; }
        public long BytesTransferred { get; internal set; }

        public long Length { get; internal set; }
    }
}
