using System;

namespace SkunkLab.VirtualRtu.Adapters
{
    public class AdapterEventArgs : EventArgs
    {
        public AdapterEventArgs(string id, Exception error = null)
        {
            ChannelId = id;
            Error = error;
        }

        public string ChannelId { get; internal set; }
        public Exception Error { get; internal set; }
    }
}
