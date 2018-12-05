using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Core
{

    [Serializable]
    [JsonObject]
    public class OrleansConfig
    {
        public OrleansConfig()
        {
        }

        [JsonProperty("dockerized")]
        public bool Dockerized { get; set; }

        [JsonProperty("orleansClusterId")]
        public string OrleansClusterId { get; set; } //orleans cluster id

        [JsonProperty("orleansServiceId")]
        public string OrleansServiceId { get; set; } //orleans service id

        [JsonProperty("orleansDataProviderType")]
        public string OrleansDataProviderType { get; set; }  //AzureStore, RedisStore

        [JsonProperty("orleansDataConnectionString")]
        public string OrleansDataConnectionString { get; set; } //Connection string that matches data provider type

        [JsonProperty("orleansDataContainerName")]
        public string OrleansDataContainerName { get; set; } //can be omitted if DataProviderType != AzureBlob

        [JsonProperty("orleansLivenessProviderType")]
        public string OrleansLivenessProviderType { get; set; } //AzureTable | Consul => Cloud vs On-Prem

        [JsonProperty("orleansLivenessConnectionString")]
        public string OrleansLivenessConnectionString { get; set; }  //connection string for liveness

        [JsonProperty("orleansMaxMemoryStorageGrains")]
        public int OrleansMaxMemoryStorageGrains { get; set; } //only use memory storage provider for development

        [JsonProperty("piraeusAuditType")]
        public string PiraeusAuditType { get; set; } //(AzureTable is the only supported option as this time.)

        [JsonProperty("piraeusAuditConnectionString")]
        public string PiraeusAuditConnectionString { get; set; } //if AuditType = AzureTable => blob connection string; otherwise omit.

        [JsonProperty("piraeusAuditTableName")]
        public string PiraeusAuditTableName { get; set;  } //can be omitted if AuditType != AzureTable

        [JsonProperty("piraeusUserLogType")]
        public string PiraeusUserLogType { get; set; }  // (AzureTable is the only support option as this time.)

        [JsonProperty("piraeusUserLogConnectionString")]
        public string PiraeusUserLogConnectionString { get; set; } //if UserLogType = AzureTable => blob connection string; otherwise omit

        [JsonProperty("piraeusUserLogTableName")]
        public string PiraeusUserLogTableName { get; set; } //can be omitted if AuditType != AzureTable

        [JsonProperty("pirauesServicePointFactor")]
        public int PirauesServicePointFactor { get; set; } //service point factor, e.g., 24 associated with Azure storage
       
    }
}
