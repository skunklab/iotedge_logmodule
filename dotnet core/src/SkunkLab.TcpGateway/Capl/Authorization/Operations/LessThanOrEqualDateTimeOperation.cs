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
    /// Compares two DateTime values to determine if the left argument is less than or equal the right argument.
    /// </summary>
    public class LessThanOrEqualDateTimeOperation : Operation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessThanOrEqualDateTimeOperation"/> class.
        /// </summary>
        public LessThanOrEqualDateTimeOperation()
        {
        }

        public static Uri OperationUri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.LessThanOrEqualDateTime); }
        }

        /// <summary>
        /// Gets the URI that identifies the operation.
        /// </summary>
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.OperationUris.LessThanOrEqualDateTime); }
        }

        /// <summary>
        /// Executes the comparsion.
        /// </summary>
        /// <param name="left">LHS of the expression argument.</param>
        /// <param name="right">RHS of the expression argument.</param>
        /// <returns>True, if the LHS argument is less than or equal the RHS argument DateTime value; otherwise false.</returns>
        public override bool Execute(string left, string right)
        {
            DateTimeComparer comparer = new DateTimeComparer();
            int result = comparer.Compare(left, right);
            return result == 0 || result == -1;
        }
    }
}
