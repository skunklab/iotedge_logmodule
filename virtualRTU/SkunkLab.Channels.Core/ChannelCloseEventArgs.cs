using System;

namespace SkunkLab.Channels.Core
{
    public class ChannelCloseEventArgs : EventArgs
    {
        public ChannelCloseEventArgs(string channelId)
        {
            ChannelId = channelId;
        }

        public string ChannelId { get; internal set; }
    }
}
