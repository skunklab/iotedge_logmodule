/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization
{
    using System;

    /// <summary>
    /// A definition of a claim.
    /// </summary>
    [Serializable]
    public class LiteralClaim 
    {        
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralClaim"/> class.
        /// </summary>
        public LiteralClaim()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralClaim"/> class.
        /// </summary>
        /// <param name="claimType">The namespace of the claim.</param>
        /// <param name="claimValue">The value of the claim.</param>
        public LiteralClaim(string claimType, string claimValue)
        {
            this.ClaimType = claimType;
            this.ClaimValue = claimValue;
        }

        /// <summary>
        /// Gets or sets the claim type.
        /// </summary>
        public string ClaimType { get; set; }
        

        /// <summary>
        /// Gets or sets the claim value.
        /// </summary>
        public string ClaimValue { get; set; }
            
    }
}
