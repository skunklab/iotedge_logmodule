using Newtonsoft.Json;
using System;

namespace LogModule.Configuration
{
    [Serializable]
    [JsonObject]
    public class DockerConfig
    {
        public DockerConfig()
        {
        }

        public DockerConfig(string accountName, string accountKey, int port, string features)
        {
            BlobStorageAccountName = accountName;
            BlobStorageAccountKey = accountKey;
            Port = port;
            Features = features;
        }

        private string features;
        private LogModuleFeatureFlags flags;

        [JsonProperty("blobStorageAccountName")]
        public string BlobStorageAccountName { get; set; }

        [JsonProperty("blobStorageAccountKey")]
        public string BlobStorageAccountKey { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("features")]
        public string Features
        {
            get { return features; }
            set
            {
                features = value;
                features = features.Replace(";", ",");
                flags = (LogModuleFeatureFlags)Enum.Parse(typeof(LogModuleFeatureFlags), features);
            }
        }

        [JsonIgnore]
        public LogModuleFeatureFlags FeatureFlags
        {
            get { return flags; }
        }


    }
}
