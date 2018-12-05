using System;

namespace SkunkLab.Storage
{
    public class BytesTransferredEventArgs : EventArgs
    {
        public BytesTransferredEventArgs(string containerName, string filename, long bytesTransferred, long length)
        {
            ContainerName = containerName;
            Filename = filename;
            BytesTransferred = bytesTransferred;
            Length = length;
        }

        public string ContainerName { get; internal set; }

        public string Filename { get; internal set; }

        public long BytesTransferred { get; internal set; }
        public long Length { get; internal set; }
    }
}
