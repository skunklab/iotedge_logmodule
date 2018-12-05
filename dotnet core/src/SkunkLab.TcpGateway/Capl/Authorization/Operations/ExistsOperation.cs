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
    /// Compares a value to determine if it is not null.
    /// </summary>
    public class ExistsOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsOperation"/> class.
        /// </summary>
        public ExistsOperation()
        {
        }

        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.Exists); }
        }

        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.Exists); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the LHS is not null; otherwise false.</returns>
        public override bool Execute(string left, string right)
        {
            return left != null;
        }
    }
}
