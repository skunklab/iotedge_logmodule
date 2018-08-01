using System;

namespace SkunkLab.Channels.Core
{
    public class ObserverEventArgs : EventArgs
    {
        public ObserverEventArgs(Uri resourceUri, string contentType, byte[] message)
        {
            ResourceUri = resourceUri;
            ContentType = contentType;
            Message = message;
        }

        public Uri ResourceUri { get; set; }

        public string ContentType { get; set; }

        public byte[] Message { get; set; }

    }
}
