using System;

namespace Channels
{
    public class ChannelErrorEventArgs : EventArgs
    {
        public ChannelErrorEventArgs(string channelId, Exception error)
        {
            ChannelId = channelId;
            Error = error;
        }
        public string ChannelId { get; internal set; }

        public Exception Error { get; internal set; }
    }
}
