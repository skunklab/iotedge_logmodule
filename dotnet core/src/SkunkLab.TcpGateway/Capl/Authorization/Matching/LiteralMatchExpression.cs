/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization.Matching
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    /// <summary>
    /// Matches the string literal of a claim type and optional claim value.
    /// </summary>
    public class LiteralMatchExpression : MatchExpression
    {

        public static Uri MatchUri
        {
            get { return new Uri(AuthorizationConstants.MatchUris.Literal); }
        }

        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.MatchUris.Literal); }
        }

        public override IList<Claim> MatchClaims(IEnumerable<Claim> claims, string claimType, string claimValue)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            ClaimsIdentity ci = new ClaimsIdentity(claims);
            IEnumerable<Claim> claimSet = ci.FindAll(delegate(Claim claim)
            {
                if (claimValue == null)
                {
                    return (claim.Type == claimType);
                }
                else
                {
                    return (claim.Type == claimType && claim.Value == claimValue);
                }
            });

            return new List<Claim>(claimSet);
        }

    }
}
