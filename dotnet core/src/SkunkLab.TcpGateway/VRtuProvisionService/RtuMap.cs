using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VRtuProvisionService
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

        public static RtuMap LoadFromConnectionString(string container, string filename, string connectionString)
        {
            RtuMap rmap = null;
            CloudStorageAccount acct = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient client = acct.CreateCloudBlobClient();
            CloudBlobContainer containerRef = client.GetContainerReference(container);
            Task task = containerRef.CreateIfNotExistsAsync();
            Task.WaitAll(task);
            CloudBlockBlob blobRef = containerRef.GetBlockBlobReference(filename);
            using (MemoryStream stream = new MemoryStream())
            {
                Task t0 = blobRef.DownloadToStreamAsync(stream);
                Task.WaitAll(t0);

                string jsonString = Encoding.UTF8.GetString(stream.ToArray());
                rmap = JsonConvert.DeserializeObject<RtuMap>(jsonString);
            }

            return rmap;
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
            if (!Map.ContainsKey(unitId))
            {
                Map.Add(unitId, new ResourceItem(unitId, rtuReceiveUriString, rtuSendUriString));
            }
            else
            {
                return false;
            }

            return true;
        }

        public ResourceItem GetResources(ushort unitId)
        {
            if (Map.ContainsKey(unitId))
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

        public async Task<bool> UpdateMapAsync(string containerName, string filename, string connectionString)
        {
            bool result = false;

            try
            {
                CloudStorageAccount acct = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient client = acct.CreateCloudBlobClient();
                CloudBlobContainer container = client.GetContainerReference(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(filename);
                blob.Properties.ContentType = "application/json";
                string jsonString = JsonConvert.SerializeObject(this);
                byte[] payload = Encoding.UTF8.GetBytes(jsonString);

                await blob.UploadFromByteArrayAsync(payload, 0, payload.Length);

                result = true;
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Failed to update RTU Map blob.");
                Trace.TraceError(ex.Message);
            }

            return result;

        }
    }
}
