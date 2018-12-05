using Newtonsoft.Json;
using System;

namespace Piraeus.Core.Messaging
{
    [Serializable]
    [JsonObject]
    public class CommunicationMetrics
    {
        public CommunicationMetrics()
        {
        }

        public CommunicationMetrics(string id, long messageCount, long byteCount, long errorCount, DateTime? lastMessageTimestamp, DateTime? lastErrorTimestamp, Exception lastError = null)
        {
            Id = id;
            MessageCount = messageCount;
            ByteCount = byteCount;
            ErrorCount = errorCount;
            LastMessageTimestamp = lastMessageTimestamp;
            LastErrorTimestamp = lastErrorTimestamp;
            LastError = lastError;
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("byteCount")]
        public long ByteCount { get; set; }

        [JsonProperty("messageCount")]
        public long MessageCount { get; set; }

        [JsonProperty("errorCount")]
        public long ErrorCount { get; set; }

        [JsonProperty("lastErrorTimestamp")]
        public DateTime? LastErrorTimestamp { get; set; }

        [JsonProperty("lastMessageTimestamp")]
        public DateTime? LastMessageTimestamp { get; set; }

        [JsonProperty("lastError")]
        public Exception LastError { get; set; }
    }
}
