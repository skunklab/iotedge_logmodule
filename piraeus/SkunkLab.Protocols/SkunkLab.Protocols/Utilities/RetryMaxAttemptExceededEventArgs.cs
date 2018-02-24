using System;

namespace SkunkLab.Protocols.Utilities
{
    public class RetryMaxAttemptExceededEventArgs : EventArgs
    {
        public RetryMaxAttemptExceededEventArgs(ushort timerId)
        {
            TimerId = timerId;
        }

        public ushort TimerId { get; internal set; }
    }
}
