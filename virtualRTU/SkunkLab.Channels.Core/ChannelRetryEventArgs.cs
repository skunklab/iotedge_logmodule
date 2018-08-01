using System;

namespace SkunkLab.Channels.Core
{
    public class ChannelRetryEventArgs : EventArgs
    {
        public ChannelRetryEventArgs(string id, byte[] message)
        {
            ChannelId = id;
            Message = message;
        }

        public string ChannelId { get; internal set; }

        public byte[] Message { get; internal set; }
    }
}
