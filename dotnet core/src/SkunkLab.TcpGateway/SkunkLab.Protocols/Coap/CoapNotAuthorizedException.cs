using System;
using System.Runtime.Serialization;

namespace SkunkLab.Protocols.Coap
{
    public class CoapNotAuthorizedException : Exception
    {
        public CoapNotAuthorizedException()
            : base()
        {

        }

        public CoapNotAuthorizedException(string message)
            : base(message)
        {

        }

        public CoapNotAuthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected CoapNotAuthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


    }
}
