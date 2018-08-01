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

        public EdgeConfig(string hostname, int port, string pskIdentity, byte[] psk, string signingKey, double lifetimeMinutes, string issuer, string audience, int blockSize, int maxBufferSize, int keepAliveSeconds, Claimset claims, RtuMap rtuMap, string moduleConnectionString)
        {
            Hostname = hostname;
            Port = port;
            PskIdentity = pskIdentity;
            Psk = psk;
            SigningKey = signingKey;
            Audience = audience;
            Issuer = Issuer;
            LifetimeMinutes = lifetimeMinutes;
            BlockSize = blockSize;
            MaxBufferSize = maxBufferSize;
            KeepAliveInterval = keepAliveSeconds;
            Claims = claims;
            Map = rtuMap;
            ModuleConnectionString = moduleConnectionString;
        }

     
        [JsonProperty("moduleConnectionString")]
        public string ModuleConnectionString { get; set; }


        /// <summary>
        /// Host name of the Piraeus TCP gateway
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// Port of the Piraeus TCP gateway (8883 is secure MQTT)
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        /// PSK Identity string that maps the identty to the PSK bytes.
        /// </summary>
        [JsonProperty("pskIdentity")]
        public string PskIdentity { get; set; }

        /// <summary>
        /// Preshared Key (PSK) to use for TLSv1.2
        /// </summary>
        [JsonProperty("psk")]
        public byte[] Psk { get; set; }



        /// <summary>
        /// Block size of TCP message chunk.
        /// </summary>
        [JsonProperty("blockSize")]
        public int BlockSize { get; set; }

        /// <summary>
        /// Max size of TCP message.
        /// </summary>
        [JsonProperty("maxBufferSize")]
        public int MaxBufferSize { get; set; }

        /// <summary>
        /// Number of idle seconds before MQTT Ping is used to keep TCP connection alive.
        /// </summary>
        [JsonProperty("keepaliveInterval")]
        public int KeepAliveInterval { get; set; }

        /// <summary>
        /// The map of the RTU Unit ID to Piraeus resources for send/receive.
        /// </summary>
       [JsonIgnore]
        public RtuMap Map
        {
            get
            {
                if(!string.IsNullOrEmpty(MapString))
                {
                    return JsonConvert.DeserializeObject<RtuMap>(MapString);
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if(value != null)
                {
                    MapString = JsonConvert.SerializeObject(value);
                }
            }
        }

        [JsonProperty("rtuMap")]
        public string MapString { get; set; }

        [JsonProperty("signingKey")]
        public string SigningKey { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("lifetimeMinutes")]
        public double LifetimeMinutes { get; set; }


        [JsonIgnore]
        public Claimset Claims
        {
            get
            {
                if(!string.IsNullOrEmpty(ClaimsString))
                {
                    return JsonConvert.DeserializeObject<Claimset>(ClaimsString);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if(value != null)
                {
                    ClaimsString = JsonConvert.SerializeObject(value);
                }
            }
        }

        [JsonProperty("claims")]
        public string ClaimsString { get; set; }


        public string GetSecurityToken()
        {
            JsonWebToken token = new JsonWebToken(SigningKey, Claims.ToClaims(), LifetimeMinutes, Issuer, Audience);
            return token.ToString();
        }


    }
}
