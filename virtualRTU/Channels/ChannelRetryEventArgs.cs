using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels
{
    public class ChannelRetryEventArgs : EventArgs
    {
        public ChannelRetryEventArgs(string id, byte[] message)
        {
            ChannelId = id;
            Message = message;
        }

        public string ChannelId { get; internal set; }

        public byte[] Message { get; internal set; }
    }
}
