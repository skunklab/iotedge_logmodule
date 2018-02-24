using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace SkunkLab.Diagnostics.Audit
{
    public class AuditRecord : TableEntity
    {
        public AuditRecord()
        {
        }

        public AuditRecord(string messageId, string identity, string channel, string protocol, int length, DateTime messageTime)
        {
            MessageId = messageId;
            Identity = identity;
            Channel = channel;
            Protocol = protocol;
            Length = length;
            Direction = "In";
            MessageTime = messageTime;
        }

        public AuditRecord(string messageId, string identity, string channel, string protocol, int length, bool success, DateTime messageTime, string error = null)
        {
            MessageId = messageId;
            Identity = identity;
            Channel = channel;
            Protocol = protocol;
            Length = length;
            Direction = "Out";
            Error = error;
            Success = success;
            MessageTime = messageTime;
        }


        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("identity")]
        public string Identity { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("messageTimestamp")]
        public DateTime MessageTime { get; set; }
    }
}
