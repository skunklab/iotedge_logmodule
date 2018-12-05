using System;
using System.Runtime.Serialization;

namespace FieldGatewayMicroservice.Communications
{
    public class CommunicationsException : Exception
    {
        public CommunicationsException()
        {

        }

        public CommunicationsException(string message)
            : base(message)
        {

        }

        public CommunicationsException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected CommunicationsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
