using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.Edge.Gateway
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
