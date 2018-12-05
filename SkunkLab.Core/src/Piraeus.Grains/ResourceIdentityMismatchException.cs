using System;
using System.Runtime.Serialization;

namespace Piraeus.Grains
{
    [Serializable]
    public class ResourceIdentityMismatchException : Exception
    {
        public ResourceIdentityMismatchException()
        {

        }

        public ResourceIdentityMismatchException(string message)
            : base(message)
        {

        }

        public ResourceIdentityMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected ResourceIdentityMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}

