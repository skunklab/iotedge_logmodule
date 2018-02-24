
namespace Piraeus.Protocols.Mqtt
{
    using System;

    public class MqttTimerEventArgs : EventArgs
    {
        public MqttTimerEventArgs()
        {
        }

        public MqttTimerEventArgs(ushort messageId, int retryCount)
        {
            this.MessageId = messageId;
            this.RetryCount = retryCount;
        }

        public ushort MessageId { get; internal set; }

        public int RetryCount { get; internal set; }
    }
}
