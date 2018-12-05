using Newtonsoft.Json;
using System;

namespace LogModule.Models
{
    [Serializable]
    [JsonObject]
    public class CompressFileModel
    {
        public CompressFileModel()
        {
        }

        public CompressFileModel(string path, string filename, string compressPath, string compressFilename)
        {
            Path = path;
            Filename = filename;
            CompressPath = compressPath;
            CompressFilename = compressFilename;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("compressPath")]
        public string CompressPath { get; set; }

        [JsonProperty("compressFilename")]
        public string CompressFilename { get; set; }
    }
}
