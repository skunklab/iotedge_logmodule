using System;
using System.Runtime.Serialization;

namespace SkunkLab.Core
{
    public class ProtocolAdapterPortException : Exception
    {
        public ProtocolAdapterPortException()
            : base()
        {
        }

        public ProtocolAdapterPortException(string message)
           :base(message)
        {
        }

        public ProtocolAdapterPortException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ProtocolAdapterPortException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
