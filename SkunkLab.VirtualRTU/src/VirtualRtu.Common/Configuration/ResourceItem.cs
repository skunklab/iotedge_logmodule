using Newtonsoft.Json;

namespace VirtualRtu.Common.Configuration
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

        [JsonProperty("unitId")]
        public int UnitId { get; set; }
    }
}
