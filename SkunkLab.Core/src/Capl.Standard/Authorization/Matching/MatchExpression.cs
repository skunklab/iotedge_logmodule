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


    public abstract class MatchExpression
    {
        public abstract Uri Uri { get; }

        public abstract IList<Claim> MatchClaims(IEnumerable<Claim> claims, string claimType, string value);

        public static MatchExpression Create(Uri matchType, MatchExpressionDictionary matchExpressions)
        {
            if (matchType == null)
            {
                throw new ArgumentNullException("matchType");
            }

            MatchExpression matchExpression = null;

            if (matchExpressions == null)
            {
                matchExpression = MatchExpressionDictionary.Default[matchType.ToString()]; //CaplConfigurationManager.MatchExpressions[matchType.ToString()];
            }
            else
            {
                matchExpression = matchExpressions[matchType.ToString()];
            }

            return matchExpression;
        }
    }
}
