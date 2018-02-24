using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Mqtt
{
    public sealed class MqttConfig
    {

        public MqttConfig(double ackTimeout = 2.0, double ackRandomFactor = 1.5, int maxRetransmit = 4, double maxLatency = 100.0)
        {
            AckTimeout = TimeSpan.FromSeconds(ackTimeout);
            AckRandomFactor = ackRandomFactor;
            MaxRetransmit = maxRetransmit;
            MaxLatency = TimeSpan.FromSeconds(maxLatency);
        }

        public TimeSpan AckTimeout { get; internal set; }
        public double AckRandomFactor { get; internal set; }

        public int MaxRetransmit { get; internal set; }

        public TimeSpan MaxTransmitSpan
        {
            get
            {
                //ACK_TIMEOUT * (( 2 ** MAX_RETRANSMIT) - 1) * ACK_RANDOM_FACTOR
                double secs = (AckTimeout.TotalSeconds) * (Math.Pow(2.0, Convert.ToDouble(MaxRetransmit)) - 1) * AckRandomFactor;
                return TimeSpan.FromSeconds(secs);
            }
        }

        public TimeSpan MaxTransmitWait
        {
            get
            {
                //ACK_TIMEOUT * (( 2 ** (MAX_RETRANSMIT + 1)) - 1) * ACK_RANDOM_FACTOR
                return TimeSpan.FromSeconds(AckTimeout.TotalSeconds * (Math.Pow(2.0, Convert.ToDouble(MaxRetransmit) + 1) - 1) * AckRandomFactor);
            }
        }

        public TimeSpan MaxLatency { get; internal set; }

      
        public TimeSpan ExchangeLifetime
        {
            get
            {
                //MAX_TRANSMIT_SPAN + (2 * MAX_LATENCY) + PROCESSING_DELAY
                return TimeSpan.FromSeconds(MaxTransmitSpan.TotalSeconds + (2 * MaxLatency.TotalSeconds) + AckTimeout.TotalSeconds);
            }
        }

        public TimeSpan NonLifetime
        {
            get
            {
                //MAX_TRANSMIT_SPAN + MAX_LATENCY
                return TimeSpan.FromSeconds(MaxTransmitSpan.TotalSeconds + MaxLatency.TotalSeconds);
            }
        }


    }
    //public class MqttConfig
    //{
    //    public MqttConfig(int maxRetryAttempts, TimeSpan retryDelay, TimeSpan maxTransmitSpan)
    //    {
    //        MaxRetryAttempts = maxRetryAttempts;
    //        RetryDelay = retryDelay;
    //        MaxTransmitSpan = maxTransmitSpan;
    //    }

    //    public int MaxRetryAttempts { get; internal set; }

    //    public TimeSpan RetryDelay { get; internal set; }

    //    public TimeSpan MaxTransmitSpan { get; internal set; }
    //}
}
