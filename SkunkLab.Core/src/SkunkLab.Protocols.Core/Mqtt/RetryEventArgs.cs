
namespace SkunkLab.Protocols.Mqtt
{
    using System;

    public class RetryEventArgs : EventArgs
    {
        public RetryEventArgs(MqttMessage message)
        {
            Message = message;
        }

        public MqttMessage Message { get; internal set; }
    }
}
