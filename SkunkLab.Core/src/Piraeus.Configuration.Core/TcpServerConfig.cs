using Newtonsoft.Json;
using Piraeus.Configuration.Settings;
using System;

namespace Piraeus.Configuration.Core
{
    [Serializable]
    [JsonObject]
    public class TcpServerConfig
    {
        public TcpServerConfig()
        {

        }

        public TcpServerConfig(TcpSettings settings)
        {
            Settings = settings;
        }

        [JsonProperty("tcp")]
        public TcpSettings Settings { get; set; }


        
    }
}
