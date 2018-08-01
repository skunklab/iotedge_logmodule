using System;
using System.Runtime.Serialization;

namespace SkunkLab.Protocols.Coap
{
    public class CoapRstException : Exception
    {
        public CoapRstException()
            : base()
        {

        }

        public CoapRstException(string message)
            : base(message)
        {

        }

        public CoapRstException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected CoapRstException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
