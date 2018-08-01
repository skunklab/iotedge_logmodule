using System;

namespace Channels
{
    public class ComAdapterEventArgs : EventArgs
    {
        public ComAdapterEventArgs(string channelId, Exception error = null)
        {
            ChannelId = channelId;
            Error = error;
        }

        public string ChannelId { get; internal set; }

        public Exception Error { get; internal set; }
    }
}
