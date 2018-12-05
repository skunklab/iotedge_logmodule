using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.VirtualRtu.Adapters
{
    [JsonObject]
    public class ResourceItem
    {
        public ResourceItem()
        {

        }

        public ResourceItem(string rtuInputResource, string rtuOutputResource)
        {
            RtuInputResource = rtuInputResource;
            RtuOutputResource = rtuOutputResource;
        }

        [JsonProperty("rtuInput")]
        public string RtuInputResource { get; set; }

        [JsonProperty("rtuOutput")]
        public string RtuOutputResource { get; set; }
    }
}
