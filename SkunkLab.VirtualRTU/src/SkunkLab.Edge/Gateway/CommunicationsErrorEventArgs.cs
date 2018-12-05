using System;

namespace SkunkLab.Edge.Gateway
{
    public class CommunicationsErrorEventArgs : EventArgs
    {
        public CommunicationsErrorEventArgs(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; internal set; }

        public DateTime Timestamp { get; internal set; }
    }
}
