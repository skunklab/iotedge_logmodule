using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.VirtualRtu.Listener
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
