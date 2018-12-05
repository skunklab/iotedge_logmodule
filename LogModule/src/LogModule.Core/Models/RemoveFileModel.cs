using Newtonsoft.Json;
using System;

namespace LogModule.Models
{
    [Serializable]
    [JsonObject]
    public class RemoveFileModel
    {
        public RemoveFileModel()
        {

        }

        public RemoveFileModel(string path, string filename)
        {
            Path = path;
            Filename = filename;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }
    }
}
