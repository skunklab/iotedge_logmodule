using System;

namespace SkunkLab.Channels.Core
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
