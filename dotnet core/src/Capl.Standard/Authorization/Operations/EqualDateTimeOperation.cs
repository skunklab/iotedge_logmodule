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
    /// Compares two DateTime parameters for equality.
    /// </summary>
    public class EqualDateTimeOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualDateTimeOperation"/> class.
        /// </summary>
        public EqualDateTimeOperation()
        {
        }

        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.EqualDateTime); }
        }

        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.EqualDateTime); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the arguments are equal DateTime values.</returns>
        public override bool Execute(string left, string right)
        {
            DateTimeComparer comparer = new DateTimeComparer();
            return comparer.Compare(left, right) == 0;
        }
    }
}
