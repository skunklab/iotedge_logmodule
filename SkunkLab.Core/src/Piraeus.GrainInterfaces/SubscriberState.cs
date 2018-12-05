using System;
using System.Collections.Generic;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class SubscriberState
    {
        public List<string> Container { get; set; }
    }
}
