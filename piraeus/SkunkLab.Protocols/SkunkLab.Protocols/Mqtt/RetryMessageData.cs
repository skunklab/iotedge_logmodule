using System;

namespace SkunkLab.Protocols.Mqtt
{
    internal class RetryMessageData
    {
        public RetryMessageData(MqttMessage message, DateTime nextRetryTime, int attempts)
        {
            Message = message;
            NextRetryTime = nextRetryTime;
            AttemptCount = attempts;
        }

        public MqttMessage Message { get; set; }

        public DateTime NextRetryTime { get; set; }

        public int AttemptCount { get; set; }

        public void Increment(TimeSpan ackTimeout)
        {
            AttemptCount++;
            double nextTimeoutDuration = Math.Pow(2.0, Convert.ToDouble(AttemptCount)) * ackTimeout.TotalMilliseconds;
            NextRetryTime.Add(TimeSpan.FromMilliseconds(nextTimeoutDuration));
        }
    }
}
