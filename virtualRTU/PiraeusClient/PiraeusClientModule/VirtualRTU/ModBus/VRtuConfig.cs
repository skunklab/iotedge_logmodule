using Newtonsoft.Json;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Claims;
using System.Text;

namespace VirtualRTU.ModBus
{
    [JsonObject]
    [Serializable]
    public class VRtuConfig
    {
        public VRtuConfig()
        {

        }


        public static VRtuConfig Load()
        {
            //read the file
            string jsonString = Encoding.UTF8.GetString(File.ReadAllBytes("settings.config.json"));
            return JsonConvert.DeserializeObject<VRtuConfig>(jsonString);
        }

        [JsonProperty("piraeus_hostname")]
        public string PiraeusHostname { get; set; }

        [JsonProperty("psk_identifier")]
        public string PskIdentifier { get; set; }

        [JsonProperty("psk")]
        public string Psk { get; set; }

        [JsonProperty("security_token_type")]
        public string SecurityTokenType { get; set; }

        [JsonProperty("security_key")]
        public string SecurityTokenKey { get; set; }

        [JsonProperty("token_issuer")]
        public string Issuer { get; set; }

        [JsonProperty("token_audience")]
        public string Audience { get; set; }

        [JsonProperty("token_lifetime_minutes")]
        public TimeSpan TokenLifeTimeMinutes { get; set; }

        [JsonProperty("token_claims")]
        public JsonClaim[] Claims { get; set; }

        [JsonProperty("tcp_max_buffer_size")]
        public int TcpMaxBufferSize { get; set; }        

        [JsonProperty("tcp_block_size")]
        public int TcpBlockSize { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("rtu_map")]
        public RtuMapItem[] Map { get; set; }

        [JsonIgnore]
        public byte[] PskBytes
        {
            get { return Convert.FromBase64String(this.Psk); }
        }

        public string GetSecurityToken()
        {
            if(SecurityTokenType != "JWT")
            {
                throw new SecurityException("Invalid security token type.  Expected JWT.");
            }

            List<Claim> claimset = new List<Claim>();
            foreach(JsonClaim item in Claims)
            {
                claimset.Add(new Claim(item.Type, item.Value));
            }



            JsonWebToken token = new JsonWebToken(new Uri(Audience), SecurityTokenKey, Issuer, claimset, TokenLifeTimeMinutes.TotalMinutes);
            return token.ToString();
        }

        
    }
}
