using Newtonsoft.Json;
using SkunkLab.Security.Tokens;
using SkunkLab.VirtualRtu.Adapters;

namespace SkunkLab.Edge.Gateway.Mqtt
{
    /// <summary>
    /// Edge client configuration for Piraeus TCP gateway communications with Virtual RTU.
    /// </summary>
    [JsonObject]
    public class EdgeConfig
    {
        public EdgeConfig()
        {
        }
        
        
        [JsonProperty("luss")]
        public string LUSS { get; set; }


        [JsonProperty("serviceUrl")]
        public string ServiceUrl { get; set; }

    }
}
