using SkunkLab.Protocols.Mqtt.Handlers;
using System;

namespace SkunkLab.Protocols.Mqtt
{
    internal class RetryMessageData
    {
        public RetryMessageData(MqttMessage message, DateTime nextRetryTime, int attempts, DirectionType direction)
        {
            Message = message;
            NextRetryTime = nextRetryTime;
            AttemptCount = attempts;
            Direction = direction;
        }



        public MqttMessage Message { get; set; }

        public DateTime NextRetryTime { get; set; }

        public int AttemptCount { get; set; }

        public DirectionType Direction { get; set; }

        public void Increment(TimeSpan ackTimeout)
        {
            AttemptCount++;
            double nextTimeoutDuration = Math.Pow(2.0, Convert.ToDouble(AttemptCount)) * ackTimeout.TotalMilliseconds;
            NextRetryTime.Add(TimeSpan.FromMilliseconds(nextTimeoutDuration));
        }
        
    }
}
