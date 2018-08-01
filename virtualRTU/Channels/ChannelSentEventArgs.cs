using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Channels
{
    public class ChannelSentEventArgs : EventArgs
    {
        public ChannelSentEventArgs(string channelId, string messageId)
        {
            ChannelId = channelId;
            MessageId = messageId;

        }

        public string ChannelId { get; internal set; }

        public string MessageId { get; internal set; }
    }
}
