/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Issuance
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class IssueModeNotRecognizedException : Exception
    {
        public IssueModeNotRecognizedException()
            : base()
        {
        }

        public IssueModeNotRecognizedException(string message)
            : base(message)
        {
        }

        public IssueModeNotRecognizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected IssueModeNotRecognizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
