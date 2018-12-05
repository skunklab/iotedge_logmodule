using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkunkLab.VirtualRtu.Adapters
{
    [Serializable]
    [JsonObject]
    public class RtuMap
    {
        public RtuMap()
        {
            Map = new Dictionary<ushort, ResourceItem>();
        }


        public static RtuMap LoadFromEnvironmentVariable()
        {
            string jsonString = System.Environment.GetEnvironmentVariable("RTU_MAP_JSON");
            
            if (!string.IsNullOrEmpty(jsonString))
            {                
                return JsonConvert.DeserializeObject<RtuMap>(jsonString);
            }
            else
            {
                string uriString = System.Environment.GetEnvironmentVariable("RTU_MAP_SAS_URI");
                return LoadFromConnectionString(uriString);
            }
        }

        public static RtuMap LoadFromConnectionString(string uriString)
        {
            Task<RtuMap> task = ReadBlobAsync(uriString);
            Task.WaitAll(task);
            return task.Result;
        }


        private static async Task<RtuMap> ReadBlobAsync(string uriString)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage message = await client.GetAsync(uriString);
            string jsonString = await message.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RtuMap>(jsonString);
        }

        [JsonProperty("map")]
        public Dictionary<ushort, ResourceItem> Map { get; set; }


        public bool AddResource(ushort unitId, string rtuReceiveUriString, string rtuSendUriString)
        {
            if(!Map.ContainsKey(unitId))
            {
                Map.Add(unitId, new ResourceItem(rtuReceiveUriString, rtuSendUriString));
            }
            else
            {
                return false;
            }

            return true;
        }

        public ResourceItem GetResources(ushort unitId)
        {
            if(Map.ContainsKey(unitId))
            {
                return Map[unitId];
            }

            return null;
        }

        public bool HasResources(ushort unitId)
        {
            return Map.ContainsKey(unitId);
        }

        public bool RemoveResource(ushort unitId)
        {
            return Map.Remove(unitId);
        }
    }
}
