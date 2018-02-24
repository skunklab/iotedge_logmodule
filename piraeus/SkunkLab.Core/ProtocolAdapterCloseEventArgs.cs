using System;

namespace SkunkLab.Core
{
    public class ProtocolAdapterCloseEventArgs : EventArgs
    {
        public ProtocolAdapterCloseEventArgs(string channelId)
        {
            ChannelId = channelId;
        }

        public string ChannelId { get; set; }
    }
}
