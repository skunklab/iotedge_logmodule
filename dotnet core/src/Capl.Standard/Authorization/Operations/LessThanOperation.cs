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
    /// Compares two decimal values to determine if the left argument is less than the right argument.
    /// </summary>
    public class LessThanOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOperation"/> class.
        /// </summary>
        public LessThanOperation()
        {
        }


        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.LessThan); }
        }
        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.LessThan); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the LHS argument is less than the RHS argument decimal value; otherwise false.</returns>
        public override bool Execute(string left, string right)
        {
            DecimalComparer dc = new DecimalComparer();
            return dc.Compare(left, right) == -1;
        }
    }
}
