using System;
using Piraeus.Core.Messaging;

namespace Piraeus.Grains
{
    public class MetricNotificationEventArgs : EventArgs
    {
        public MetricNotificationEventArgs()
        {

        }
        public MetricNotificationEventArgs(CommunicationMetrics metrics)
        {
            Metrics = metrics;
        }

        public CommunicationMetrics Metrics { get; internal set; }
    }
}
