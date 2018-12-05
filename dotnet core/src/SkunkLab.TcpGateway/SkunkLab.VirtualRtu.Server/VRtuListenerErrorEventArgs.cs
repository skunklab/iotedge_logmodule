using System;

namespace SkunkLab.VirtualRtu.Server
{
    public class VRtuListenerErrorEventArgs : EventArgs
    {
        public VRtuListenerErrorEventArgs(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; internal set; }
    }
}
