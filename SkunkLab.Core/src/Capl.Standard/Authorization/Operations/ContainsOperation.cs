/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/



namespace Capl.Authorization.Operations
{
    using System;

    /// <summary>
    /// Determines if a string is a substring on another.
    /// </summary>
    public class ContainsOperation : Operation
    {
        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.Contains); }
        }

        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.Contains); }
        }

        public override bool Execute(string left, string right)
        {
            return left.Contains(right);
        }
    }
}
