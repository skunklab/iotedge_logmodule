using System;
using System.Runtime.Serialization;

namespace SkunkLab.Edge.Gateway
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
