using System;

namespace Piraeus.Grains
{
    public class ErrorNotificationEventArgs : EventArgs
    {
        public ErrorNotificationEventArgs()
        {
        }

        public ErrorNotificationEventArgs(string id, Exception error)
        {
            Id = id;
            Error = error;
        }

        public string Id { get; internal set; }

        public Exception Error { get; internal set; }
    }
}
