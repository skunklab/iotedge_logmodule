using System;

namespace SkunkLab.Edge.Gateway.Modules
{
    public class ModuleMessageEventArgs : EventArgs
    {
        public ModuleMessageEventArgs(byte[] message)
        {
            Message = message;
            Timestamp = DateTime.Now;
        }

        public byte[] Message { get; internal set; }

        public DateTime Timestamp { get; internal set; }
    }
}
