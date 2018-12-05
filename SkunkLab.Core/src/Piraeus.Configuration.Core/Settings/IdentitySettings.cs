using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{

    [Serializable]
    [JsonObject]
    public class IdentitySettings
    {
        public IdentitySettings()
        {
        }

        public IdentitySettings(ClientIdentity client, ServiceIdentity service)
        {
            Client = client;
            Service = service;
        }

        [JsonProperty("client")]
        public ClientIdentity Client { get; set; }

        [JsonProperty("service")]
        public ServiceIdentity Service { get; set; }
    }
}
