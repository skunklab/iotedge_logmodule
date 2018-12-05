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
    /// Compares two string for inequality.
    /// </summary>
    public class NotEqualOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotEqualOperation"/> class.
        /// </summary>
        public NotEqualOperation()
        {
        }

        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.NotEqual); }
        }

        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.NotEqual); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the arguments are not equal string values; otherwise false.</returns>
        public override bool Execute(string left, string right)
        {
            return left != right;
        }
    }
}
