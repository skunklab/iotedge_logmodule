using System;
using System.Runtime.Serialization;

namespace Piraeus.Adapters
{
    public class DisconnectException : Exception
    {
        public DisconnectException()
            : base()
        {
        }

        public DisconnectException(string message)
           : base(message)
        {
        }

        public DisconnectException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DisconnectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
