using System;

namespace LogModule.Clients
{
    public class GetFileEventArgs : EventArgs
    {
        public GetFileEventArgs(byte[] content)
        {
            Content = content;
        }

        public byte[] Content { get; internal set; }
    }
}
