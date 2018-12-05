using Newtonsoft.Json;

namespace VRtuProvisionService
{
    [JsonObject]
    public class ResourceItem
    {
        public ResourceItem()
        {

        }

        public ResourceItem(int unitId, string rtuInputResource, string rtuOutputResource)
        {
            UnitId = unitId;
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
