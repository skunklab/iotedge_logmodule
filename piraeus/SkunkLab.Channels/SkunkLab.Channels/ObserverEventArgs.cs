using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Channels
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
