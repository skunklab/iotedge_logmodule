using System;
using System.Collections.Generic;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class ResourceListState
    {
        public List<string> Container { get; set; }
    }
}
