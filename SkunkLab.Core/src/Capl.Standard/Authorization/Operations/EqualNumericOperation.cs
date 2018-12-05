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
    /// Compares two decimals for equality.
    /// </summary>
    public class EqualNumericOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualNumericOperation"/> class.
        /// </summary>
        public EqualNumericOperation()
        {
        }

        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.EqualNumeric); }
        }

        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.EqualNumeric); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the arguments are equal decimal values; othewise false.</returns>
        public override bool Execute(string left, string right)
        {
            DecimalComparer dc = new DecimalComparer();
            return dc.Compare(left, right) == 0;
        }
    }
}
