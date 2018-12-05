using Newtonsoft.Json;
using System;

namespace LogModule.Models
{
    [Serializable]
    [JsonObject]
    public class DownloadFileModel
    {
        public DownloadFileModel()
        {

        }

        public DownloadFileModel(string path, string filename, string blobPath, string blobFilename, bool append = false, bool cancel = false)
        {
            Path = path;
            Filename = filename;
            BlobPath = blobPath;
            BlobFilename = blobFilename;
            Append = append;
            Cancel = cancel;
        }

        public DownloadFileModel(string path, string filename, string sasUri, bool append = false, bool cancel = false)
        {
            Path = path;
            Filename = filename;
            SasUri = sasUri;
            Append = append;
            Cancel = cancel;
        }


        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("blobPath")]
        public string BlobPath { get; set; }

        [JsonProperty("blobFilename")]
        public string BlobFilename { get; set; }

        [JsonProperty("sasUri")]
        public string SasUri { get; set; }

        [JsonProperty("append")]
        public bool Append { get; set; }

        [JsonProperty("cancel")]
        public bool Cancel { get; set; }


    }
}
