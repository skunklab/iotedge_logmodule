using System;

namespace LogModule
{
    public class BlobCompleteEventArgs : EventArgs
    {
        public BlobCompleteEventArgs(string filename, bool canceled = false)
        {
            Filename = filename;
            Cancelled = canceled;
        }

        public string Filename { get; internal set; }

        public bool Cancelled { get; internal set; }
    }
}
