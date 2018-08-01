using System;

namespace Channels
{
    public class ChannelStateEventArgs : EventArgs
    {
        public ChannelStateEventArgs(string channelId, ChannelState state)
        {
            ChannelId = channelId;
            State = state;
        }

        public string ChannelId { get; internal set; }

        public ChannelState State { get; internal set; }
    }
}
