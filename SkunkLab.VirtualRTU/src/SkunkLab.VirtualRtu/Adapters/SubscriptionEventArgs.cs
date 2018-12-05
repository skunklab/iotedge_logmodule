using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.VirtualRtu.Adapters
{
    public class SubscriptionEventArgs : EventArgs
    {
        public SubscriptionEventArgs(string resourceUriString, string contentType, byte[] message)
        {
            ResourceUriString = resourceUriString;
            ContentType = contentType;
            Message = message;
        }

        public string ResourceUriString { get; internal set; }

        public string ContentType { get; internal set; }

        public byte[] Message { get; internal set; }
    }
}
