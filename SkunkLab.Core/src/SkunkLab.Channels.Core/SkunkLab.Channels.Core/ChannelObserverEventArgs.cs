using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    public class ChannelObserverEventArgs : EventArgs
    {
        public ChannelObserverEventArgs(string resourceUriString, string contentType, byte[] message)
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
