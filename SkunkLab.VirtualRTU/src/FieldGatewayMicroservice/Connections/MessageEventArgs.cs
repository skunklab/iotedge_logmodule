using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FieldGatewayMicroservice.Connections
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(byte[] message)
        {
            Message = message;
        }

        public byte[] Message { get; internal set; }
    }
}
