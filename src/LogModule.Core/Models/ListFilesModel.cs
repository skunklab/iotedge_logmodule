using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogModule.Models
{
    [Serializable]
    [JsonObject]
    public class ListFilesModel
    {
        public ListFilesModel()
        {

        }

        public ListFilesModel(string path)
        {
            Path = path;
        }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
