using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class ClientSecurity
    {
        public ClientSecurity()
        {
        }

        public ClientSecurity(string tokenType, string symmetricKey, string issuer = null, string audience = null)
        {
            TokenType = tokenType;
            SymmetricKey = symmetricKey;
            Issuer = issuer;
            Audience = audience;
        }

        [JsonProperty("tokenType")]
        public string TokenType { get; set; }

        [JsonProperty("symmetricKey")]
        public string SymmetricKey { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }
    }
}
