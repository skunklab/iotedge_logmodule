

namespace Piraeus.Protocols.Coap
{
    using System;
    using System.Runtime.Serialization;

     [Serializable]
    public class CoapVersionMismatchException : Exception
    {
        public CoapVersionMismatchException()
        {
        }

        public CoapVersionMismatchException(string message)
            : base(message)
        {
        }

        public CoapVersionMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CoapVersionMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
