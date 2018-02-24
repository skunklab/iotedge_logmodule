using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Channels
{
    public class ChannelReceivedEventArgs : EventArgs
    {
        public ChannelReceivedEventArgs(string channelId, byte[] message)
        {
            ChannelId = channelId;
            Message = message;
        }

        public ChannelReceivedEventArgs(string channelId, byte[] message, IEnumerable<KeyValuePair<string, string>> properties)
        {
            ChannelId = channelId;
            Message = message;
            Properties = properties;
        }

        public string ChannelId { get; internal set; }

        public byte[] Message { get; internal set; }

        public IEnumerable<KeyValuePair<string,string>> Properties { get; internal set; }
    }
}
