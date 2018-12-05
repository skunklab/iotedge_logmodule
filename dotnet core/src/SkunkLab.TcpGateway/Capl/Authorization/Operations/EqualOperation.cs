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
    /// Compares two strings for equality.
    /// </summary>
    public class EqualOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualOperation"/> class.
        /// </summary>
        public EqualOperation()
        {
        }

        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.Equal); }
        }

        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.Equal); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the arguments are equal string values; otherwise false.</returns>
        public override bool Execute(string left, string right)
        {
            return left == right;
        }
    }
}
