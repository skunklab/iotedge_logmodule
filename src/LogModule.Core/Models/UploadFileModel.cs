using Newtonsoft.Json;
using System;

namespace LogModule.Models
{
    [Serializable]
    [JsonObject]
    public class UploadFileModel
    {
        public UploadFileModel()
        {
        }


        public UploadFileModel(string path, string filename, string blobPath, string blobFilename, string contentType, bool deleteOnUpload = false, TimeSpan? ttl = null, bool append = false, bool cancel = false)
        {
            Path = path;
            Filename = filename;
            BlobPath = blobPath;
            BlobFilename = blobFilename;
            ContentType = contentType;
            DeleteOnUpload = deleteOnUpload;
            TTL = ttl;
            Append = append;
            Cancel = cancel;
        }

        public UploadFileModel(string sourcePath, string sourceFilename, string sasUri, string contentType, bool deleteOnUpload = false, TimeSpan? ttl = null, bool append = false, bool cancel = false)
        {
            Path = sourcePath;
            Filename = sourceFilename;
            SasUri = SasUri;
            ContentType = contentType;
            DeleteOnUpload = deleteOnUpload;
            TTL = ttl;
            Append = append;
            Cancel = cancel;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("blobPath")]
        public string BlobPath { get; set; }

        [JsonProperty("blobFilename")]
        public string BlobFilename { get; set; }

        [JsonProperty("sasUri")]
        public string SasUri { get; set; }

        [JsonProperty("deleteOnUpload")]
        public bool DeleteOnUpload { get; set; }

        [JsonProperty("ttl")]
        public TimeSpan? TTL { get; set; }

        [JsonProperty("append")]
        public bool Append { get; set; }

        [JsonProperty("cancel")]
        public bool Cancel { get; set; }
    }
}
