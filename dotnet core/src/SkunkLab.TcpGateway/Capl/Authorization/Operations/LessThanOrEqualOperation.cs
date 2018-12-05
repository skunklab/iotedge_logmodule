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
    /// Compares two decimal values to determine if the left argument is less than or equal the right argument.
    /// </summary>
    public class LessThanOrEqualOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOrEqualOperation"/> class.
        /// </summary>
        public LessThanOrEqualOperation()
        {                
        }

        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.LessThanOrEqual); }
        }

        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.LessThanOrEqual); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the LHS argument is less or equal than the RHS argument decimal value; otherwise false.</returns>
        public override bool Execute(string left, string right)
        {
            DecimalComparer dc = new DecimalComparer();
            int result = dc.Compare(left, right);
            return result == 0 || result == -1;
        }
    }
}
