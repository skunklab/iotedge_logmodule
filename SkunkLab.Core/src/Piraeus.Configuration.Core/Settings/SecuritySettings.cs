using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{

    [Serializable]
    [JsonObject]
    public class SecuritySettings
    {
        public SecuritySettings()
        {
        }

        public SecuritySettings(ClientSecurity client, ServiceSecurity service = null)
        {
            Client = client;
            Service = service;
        }

        [JsonProperty("client")]
        public ClientSecurity Client { get; set; }

        [JsonProperty("service")]
        public ServiceSecurity Service { get; set; }
    }
}
