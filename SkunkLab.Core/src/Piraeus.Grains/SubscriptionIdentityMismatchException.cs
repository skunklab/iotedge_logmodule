using System;
using System.Runtime.Serialization;

namespace Piraeus.Grains
{
    [Serializable]
    public class SubscriptionIdentityMismatchException : Exception
    {
        public SubscriptionIdentityMismatchException()
        {

        }

        public SubscriptionIdentityMismatchException(string message)
            : base(message)
        {

        }

        public SubscriptionIdentityMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        protected SubscriptionIdentityMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
