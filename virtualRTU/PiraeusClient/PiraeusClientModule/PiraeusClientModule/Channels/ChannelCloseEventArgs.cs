using System;

namespace PiraeusClientModule.Channels
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
