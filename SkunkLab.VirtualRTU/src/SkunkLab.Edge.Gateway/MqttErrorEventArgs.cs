using System;

namespace SkunkLab.Edge.Gateway
{
    public class MqttErrorEventArgs : EventArgs
    {
        public MqttErrorEventArgs(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; internal set; }
    }
}
