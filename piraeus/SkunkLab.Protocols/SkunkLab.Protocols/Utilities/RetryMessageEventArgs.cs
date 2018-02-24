using System;

namespace SkunkLab.Protocols.Utilities
{
    public class RetryMessageEventArgs : EventArgs
    {
        public RetryMessageEventArgs(ushort timerId)
        {
            TimerId = timerId;
        }

        public ushort TimerId { get; internal set; }
    }
}
