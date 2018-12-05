using System;

namespace SkunkLab.Storage
{
    public class BlobCompleteEventArgs : EventArgs
    {
        public BlobCompleteEventArgs(string containerName, string filename, bool cancelled, Exception error = null)
        {
            ContainerName = containerName;
            Filename = filename;
            Cancelled = cancelled;
            Error = error;
        }

        public string ContainerName { get; internal set; }

        public string Filename { get; internal set; }

        public bool Cancelled { get; internal set; }

        public Exception Error { get; internal set; }
    }
}
