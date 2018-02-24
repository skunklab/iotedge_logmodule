using System;

namespace SkunkLab.Core
{
    public sealed class EventMessage
    {
        public EventMessage(ProtocolType protocol, string contentType, byte[] message)
        {
            Protocol = protocol;
            ContentType = contentType;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }

        public ProtocolType Protocol { get; internal set; }

        public byte[] Message { get; internal set; }

        public string ContentType { get; internal set; }

        public DateTime Timestamp { get; internal set; }


        
    }
}
