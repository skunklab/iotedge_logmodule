
using Newtonsoft.Json;
using SkunkLab.Protocols.Mqtt;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Adapters
{
    public class VRtuConfig
    {
        public VRtuConfig()
        {
        }

        public static VRtuConfig LoadFromEnvironmentVariable()
        {
            string jsonString = System.Environment.GetEnvironmentVariable("VRTU_CONFIG_JSON");

            if (!string.IsNullOrEmpty(jsonString))
            {
                return JsonConvert.DeserializeObject<VRtuConfig>(jsonString);
            }
            else
            {
                string uriString = System.Environment.GetEnvironmentVariable("VRTU_CONFIG_SAS_URI");
                return LoadFromConnectionString(uriString);
            }
            
            
        }

        public static VRtuConfig LoadFromConnectionString(string uriString)
        {
            Task<VRtuConfig> task = ReadBlobAsync(uriString);
            Task.WaitAll(task);
            return task.Result;
        }


        private static async Task<VRtuConfig> ReadBlobAsync(string uriString)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage message = await client.GetAsync(uriString);
            string jsonString = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<VRtuConfig>(jsonString);
        }

        //[JsonProperty("protocolConfig")]
        //public MqttConfig ProtocolConfig { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("pskIdentity")]
        public string PskIdentity { get; set; }

        [JsonProperty("psk")]
        public byte[] Psk { get; set; }

        [JsonProperty("blockSize")]
        public int BlockSize { get; set; }

        [JsonProperty("maxBufferSize")]
        public int MaxBufferSize { get; set; }

        [JsonProperty("securityToken")]
        public string SecurityToken { get; set; }

        [JsonProperty("keepaliveInterval")]
        public int KeepAliveInterval { get; set; }

        

       
    }
}
