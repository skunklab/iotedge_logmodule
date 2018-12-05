using System;

namespace Piraeus.Adapters
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
