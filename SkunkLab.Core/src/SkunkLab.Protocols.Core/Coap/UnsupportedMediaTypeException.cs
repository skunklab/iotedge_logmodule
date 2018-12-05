
namespace SkunkLab.Protocols.Coap
{
    using System;
    using System.Runtime.Serialization;

     [Serializable]
    public class UnsupportedMediaTypeException : Exception
    {
        public UnsupportedMediaTypeException()
            : base()
        {
        }

        public UnsupportedMediaTypeException(string message)
            : base(message)
        {
        }

        public UnsupportedMediaTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected UnsupportedMediaTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
