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
    using System.Text.RegularExpressions;
    using System.Security.Claims;

    /// <summary>
    /// Matches the string literal of a claim type and optional regular expression of the claim value.
    /// </summary>
    public class PatternMatchExpression : MatchExpression
    {
        public static Uri MatchUri
        {
            get { return new Uri(AuthorizationConstants.MatchUris.Pattern); }
        }
        public override Uri Uri
        {
            get { return new Uri(AuthorizationConstants.MatchUris.Pattern); }
        }

        public override IList<Claim> MatchClaims(IEnumerable<Claim> claims, string claimType, string pattern)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            Regex regex = new Regex(pattern);

            ClaimsIdentity ci = new ClaimsIdentity(claims);
            IEnumerable<Claim> claimSet = ci.FindAll(delegate(Claim claim)
            {
                return (claimType == claim.Type);
            });


            if (pattern == null)
            {
                return new List<Claim>(claimSet);
            }

            List<Claim> claimList = new List<Claim>();
            IEnumerator<Claim> en = claimSet.GetEnumerator();

            while (en.MoveNext())
            {
                if (regex.IsMatch(en.Current.Value))
                {
                    claimList.Add(en.Current);
                }
            }

            return claimList;
        }
    }
}
