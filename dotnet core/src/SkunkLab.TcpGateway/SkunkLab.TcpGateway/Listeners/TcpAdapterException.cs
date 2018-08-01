using System;
using System.Runtime.Serialization;

namespace SkunkLab.TcpGateway.Listeners
{
    public class TcpAdapterException : Exception
    {
        public TcpAdapterException()
        {

        }

        public TcpAdapterException(string message)
            : base(message)
        {

        }

        public TcpAdapterException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected TcpAdapterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
