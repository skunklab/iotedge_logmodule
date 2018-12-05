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
    using Capl.Authorization.Transforms;
    using System.Collections.Generic;
    using Capl.Authorization.Matching;

    /// <summary>
    /// The abstract scope of a transform.
    /// </summary>
    [Serializable]
    public class ClaimTransform : Transform
    {

        public ClaimTransform()
            : this(null, null, null, null, null)
        {
        }

        public ClaimTransform(Uri transformType, LiteralClaim targetClaim)
            : this(null, transformType, null, targetClaim, null)
        {
        }

        public ClaimTransform(Uri transformType, Match matchExpression)
            : this(null, transformType, matchExpression, null, null)
        {
        }

        public ClaimTransform(Uri transformType, Match matchExpression, LiteralClaim targetClaim)
            : this(null, transformType, matchExpression, targetClaim, null)
        {
        }

        public ClaimTransform(Uri transformId, Uri transformType, Match matchExpression, LiteralClaim targetClaim, Term evaluationExpression)
        {
            this.TransformID = transformId;
            this.Type = transformType;
            this.MatchExpression = matchExpression;
            this.TargetClaim = targetClaim;
            this.Expression = evaluationExpression;
        }


        public override Uri TransformID { get; set; }

        public override Uri Type { get; set; }

        public override Term Expression { get; set; }

        public override Match MatchExpression { get; set; }

        public override LiteralClaim TargetClaim { get; set; }
        
        public static ClaimTransform Load(XmlReader reader)
        {
            ClaimTransform trans = new ClaimTransform();
            trans.ReadXml(reader);

            return trans;
        }

        #region ITransform Members
                

        /// <summary>
        /// Executes the transform.
        /// </summary>
        /// <param name="claimSet">Input set of claims to transform.</param>
        /// <returns>Transformed set of claims.</returns>
        public override IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            TransformAction action = null;            
            IList<Claim> matchedClaims = null;
            IEnumerable<Claim> transformedClaims = null;            
            bool eval = false;
            
            action = TransformAction.Create(this.Type, null);

            if (this.MatchExpression != null)
            {
                MatchExpression matcher = MatchExpressionDictionary.Default[this.MatchExpression.Type.ToString()]; //CaplConfigurationManager.MatchExpressions[this.MatchExpression.Type.ToString()];
                matchedClaims = matcher.MatchClaims(claims, this.MatchExpression.ClaimType, this.MatchExpression.Value);
            }

            if (this.Expression == null)
            {
                eval = true;
            }
            else
            {
                eval = this.Expression.Evaluate(claims);
            }

            if (eval)
            {
                transformedClaims = action.Execute(claims, matchedClaims, this.TargetClaim);
            }

            if (transformedClaims != null)
            {
                return transformedClaims;
            }
            else
            {
                return claims;
            }
        }

        #endregion

        #region IXmlSerializable Members


        /// <summary>
        /// Reads the Xml of a scope transform.
        /// </summary>
        /// <param name="reader">An XmlReader for a scope transform.</param>
        public override void ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            reader.MoveToRequiredStartElement(AuthorizationConstants.Elements.Transform);

            this.Type = new Uri(reader.GetRequiredAttribute(AuthorizationConstants.Attributes.Type));

            while (reader.Read())
            {
                if (reader.IsRequiredStartElement(AuthorizationConstants.Elements.Match))
                {
                    this.MatchExpression = Match.Load(reader);
                }

                if (reader.IsRequiredStartElement(AuthorizationConstants.Elements.TargetClaim))
                {
                    this.TargetClaim = new LiteralClaim();
                    this.TargetClaim.ClaimType = reader.GetRequiredAttribute(AuthorizationConstants.Attributes.ClaimType);

                    if (!reader.IsEmptyElement)
                    {
                        this.TargetClaim.ClaimValue = reader.GetElementValue(AuthorizationConstants.Elements.TargetClaim);
                    }
                }

                if (reader.LocalName == AuthorizationConstants.Elements.Rule ||
                        reader.LocalName == AuthorizationConstants.Elements.LogicalAnd ||
                        reader.LocalName == AuthorizationConstants.Elements.LogicalOr)
                {
                    this.Expression = Term.Load(reader);
                }

                if (reader.IsRequiredEndElement(AuthorizationConstants.Elements.Transform))
                {
                    break;
                }
            }            
        }

        /// <summary>
        /// Writes the Xml of a scope transform.
        /// </summary>
        /// <param name="writer">An XmlWriter for the scope transform.</param>
        public override void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteStartElement(AuthorizationConstants.Elements.Transform, AuthorizationConstants.Namespaces.Xmlns);

            if (this.TransformID != null)
            {
                writer.WriteAttributeString(AuthorizationConstants.Attributes.TransformId, this.TransformID.ToString());
            }
            
            writer.WriteAttributeString(AuthorizationConstants.Attributes.Type, this.Type.ToString());

            

            if (this.MatchExpression != null)
            {
                this.MatchExpression.WriteXml(writer);
            }

            if (this.TargetClaim != null)
            {
                writer.WriteStartElement(AuthorizationConstants.Elements.TargetClaim, AuthorizationConstants.Namespaces.Xmlns);
                writer.WriteAttributeString(AuthorizationConstants.Attributes.ClaimType, this.TargetClaim.ClaimType);

                if (!string.IsNullOrEmpty(this.TargetClaim.ClaimValue))
                {
                    writer.WriteString(this.TargetClaim.ClaimValue);
                }

                writer.WriteEndElement();
            }

            if (this.Expression != null)
            {
                this.Expression.WriteXml(writer);
            }

            writer.WriteEndElement();
        }    

        #endregion

    }
}
