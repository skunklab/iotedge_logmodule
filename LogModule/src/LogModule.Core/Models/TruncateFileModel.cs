using Newtonsoft.Json;
using System;

namespace LogModule.Models
{
    [Serializable]
    [JsonObject]
    public class TruncateFileModel
    {
        public TruncateFileModel()
        {
                
        }

        public TruncateFileModel(string path, string filename, int maxBytes)
        {
            Path = path;
            Filename = filename;
            MaxBytes = maxBytes;
        }


        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("maxBytes")]
        public int MaxBytes { get; set; }


    }
}
