using System;

namespace Channels
{
    public class ChannelOpenEventArgs : EventArgs
    {
        public ChannelOpenEventArgs(string channelId, dynamic message)
        {
            ChannelId = channelId;
            Message = message;
        }

        public string ChannelId { get; internal set; }

        public dynamic Message { get; internal set; }
    }
}
