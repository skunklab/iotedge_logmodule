using System;
using System.Collections.Generic;
using SkunkLab.Protocols.Mqtt;

namespace Piraeus.Clients.Mqtt
{
    public class SubscribedTopicsEventArgs : EventArgs
    {
        public SubscribedTopicsEventArgs(List<Tuple<string, QualityOfServiceLevelType>> results)
        {
            SubscribeResults = results;
        }

        public List<Tuple<string, QualityOfServiceLevelType>> SubscribeResults { get; internal set; }
    }
}
