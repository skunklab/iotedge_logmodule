/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization.Transforms
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    
    /// <summary>
    /// An abstract action used to perform a type of transform.
    /// </summary>
    public abstract class TransformAction
    {
        /// <summary>
        /// Gets a unique URI that corresponds to the transform action.
        /// </summary>
        public abstract Uri Uri { get; }

        /// <summary>
        /// Creates a transform action.
        /// </summary>
        /// <param name="action">The identifier of the transform action to create.</param>
        /// <param name="transforms">Dictionary of transforms.</param>
        /// <returns>An action used to transform claims.</returns>
        public static TransformAction Create(Uri action, TransformsDictionary transforms)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            TransformAction transformAction = null;

            if (transforms == null)
            {
                transformAction = TransformsDictionary.Default[action.ToString()]; //CaplConfigurationManager.Transforms[action.ToString()];
            }
            else
            {
                transformAction = transforms[action.ToString()];
            }

            return transformAction;
        }

        /// <summary>
        /// Executes a transform.
        /// </summary>
        /// <param name="claimSet">A set of claims to transform.</param>
        /// <param name="sourceClaim">The source claim used in matching.</param>
        /// <param name="targetClaim">The resultant claim used in the transform.</param>
        /// <returns>Transformed set of claims.</returns>
        public abstract IEnumerable<Claim> Execute(IEnumerable<Claim> claims, IList<Claim> matchedClaims, LiteralClaim targetClaim);
    }
}
