/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization
{
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Security.Claims;
    using System.Collections.Generic;
    using Capl.Authorization.Matching;

    /// <summary>
    /// Interface used to transform claims.
    /// </summary>
    [Serializable]
    public abstract class Transform 
    {
        /// <summary>
        /// An optional URI that can identify the transform.
        /// </summary>
        public abstract Uri TransformID { get; set; }

        /// <summary>
        /// A required type the identifies the transform.
        /// </summary>
        public abstract Uri Type { get; set; }

        /// <summary>
        /// An optional evaluation expression that determines whether the transform should be processed.
        /// </summary>
        /// <remarks>If the evaluation expression is omitted, then transform will be processed; otherwise the transform will only be processed if the 
        /// evaluation expression evaluates TRUE.</remarks>
        public abstract Term Expression { get; set; }

        /// <summary>
        /// A required match expression that matches the input set of claims to determine which claims should be processed.
        /// </summary>
        public abstract Match MatchExpression { get; set; }

        /// <summary>
        /// An optional target claim that applies to any transform that adds or replaces claims for the transform.
        /// </summary>
        /// ><remarks>The target claims is used for input into the transform, which is used by the normative Add and Replace transforms
        /// to Add the target claim or replace an existing claim with the target claim.</remarks>
        public abstract LiteralClaim TargetClaim { get; set; }

        /// <summary>
        /// Transforms a set of claims.
        /// </summary>
        /// <param name="claimSet">Set of claims to transform.</param>
        /// <returns>Transformed set of claims.</returns>
        public abstract IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims);

        public abstract void ReadXml(XmlReader reader);

        public abstract void WriteXml(XmlWriter writer);

        
    }
}
