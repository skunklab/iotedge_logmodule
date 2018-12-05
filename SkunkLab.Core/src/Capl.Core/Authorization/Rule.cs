/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Security.Claims;
    using Capl.Authorization.Matching;

    /// <summary>
    /// A rule that performs an evaluation.
    /// </summary>
    [Serializable]
    [XmlSchemaProvider(null, IsAny = true)]
    public class Rule : Term
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        public Rule()
            : this(null, null, true)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        /// <param name="matchType">An expression to match claims for the operation.</param>
        /// <param name="operation">The operation that performs an evaluation.</param>
        public Rule(Match matchType, EvaluationOperation operation)
            : this(matchType, operation, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule"/> class.
        /// </summary>
        /// <param name="matchType">An expression to match claims for the operation.</param>
        /// <param name="operation">The operation that performs an evaluation.</param>
        /// <param name="evaluates">The truthful evaluation for the rule.</param>
        public Rule(Match matchType, EvaluationOperation operation, bool evaluates)
        {
            this.MatchExpression = matchType;
            this.Operation = operation;
            this.Evaluates = evaluates;
        }

        public override Uri TermId { get; set; }

        public string Issuer { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the authorization operation.
        /// </summary>
        public EvaluationOperation Operation { get; set; }

        /// <summary>
        /// Gets or sets an expression that matches claims to be evaluted.  The matching claim values
        ///  represent the left hand side operand vlaue of the authorization operation.
        /// </summary>    
        public Match MatchExpression { get; set; }

        new public static Rule Load(XmlReader reader)
        {
            Rule rule = new Rule();
            rule.ReadXml(reader);

            return rule;
        }

        #region IEvaluationRule Members

        /// <summary>
        /// Gets or sets an expression
        /// </summary>
        /// <remarks>If the evaluation of the operation matches the Evaluates property, then the evaluation of the rule is true; otherwise false.</remarks>
        public override bool Evaluates { get; set; }
        

        /// <summary>
        /// Evaluates a set of claims using the authorization operation.
        /// </summary>
        /// <param name="claimSet">The set of claims to be evaluated.</param>
        /// <returns>True if the set of claims evaluates to true; otherwise false.</returns>
        public override bool Evaluate(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            IList<Claim> list = null;
            Capl.Authorization.Operations.Operation operation = null;
            MatchExpression exp = Capl.Authorization.Matching.MatchExpression.Create(this.MatchExpression.Type, null);
            
            list = exp.MatchClaims(claims, this.MatchExpression.ClaimType, this.MatchExpression.Value);

            if (list.Count == 0)
            {
                return !this.MatchExpression.Required;
            }

            if (this.Issuer != null)
            {
                int count = list.Count;
                for (int index = 0; index < count; index++)
                {
                    if (list[index].Issuer != this.Issuer)
                    {
                        list.Remove(list[index]);
                        index--;
                        count--;
                    }
                }
            }

            operation = Capl.Authorization.Operations.Operation.Create(this.Operation.Type, null);
            
            foreach (Claim claim in list)
            {
                bool eval = operation.Execute(claim.Value, this.Operation.ClaimValue);                                              

                if (this.Evaluates && eval)
                {
                    return true;
                }

                if (!this.Evaluates && eval)
                {
                    return false;
                }
            }            

            return !this.Evaluates;
        }

        #endregion

        #region IXmlSerializable Members

        
       
        public override void ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            reader.MoveToRequiredStartElement(AuthorizationConstants.Elements.Rule);
            string termId = reader.GetOptionalAttribute(AuthorizationConstants.Attributes.TermId);

            if (!string.IsNullOrEmpty(termId))
            {
                this.TermId = new Uri(termId);
            }

            this.Issuer = reader.GetOptionalAttribute(AuthorizationConstants.Attributes.Issuer);

            string evaluates = reader.GetOptionalAttribute(AuthorizationConstants.Attributes.Evaluates);

            if (!string.IsNullOrEmpty(evaluates))
            {
                this.Evaluates = XmlConvert.ToBoolean(evaluates);
            }

            while (reader.Read())
            {
                if(reader.IsRequiredStartElement(AuthorizationConstants.Elements.Operation))
                {
                    this.Operation = EvaluationOperation.Load(reader);
                }

                if (reader.IsRequiredStartElement(AuthorizationConstants.Elements.Match))
                {
                    this.MatchExpression = Match.Load(reader);
                }

                if (reader.IsRequiredEndElement(AuthorizationConstants.Elements.Rule))
                {
                    break;
                }
            }

            reader.Read();
        }

        /// <summary>
        /// Writes the Xml of a evaluation rule.
        /// </summary>
        /// <param name="writer">An XmlWriter for the evaluation rule.</param>
        public override void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteStartElement(AuthorizationConstants.Elements.Rule, AuthorizationConstants.Namespaces.Xmlns);

            if (this.Issuer != null)
            {
                writer.WriteAttributeString(AuthorizationConstants.Attributes.Issuer, this.Issuer);
            }

            if (this.TermId != null)
            {
                writer.WriteAttributeString(AuthorizationConstants.Attributes.TermId, this.TermId.ToString());
            }

            writer.WriteAttributeString(AuthorizationConstants.Attributes.Evaluates, XmlConvert.ToString(this.Evaluates));

            this.Operation.WriteXml(writer);

            this.MatchExpression.WriteXml(writer);

            writer.WriteEndElement();            
        }

        #endregion
    }
}
