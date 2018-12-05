/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Security.Claims;
    using System.Collections.Generic;

    /// <summary>
    /// Performs a logical disjunction (Logical OR) on a collection of objects implementing IEvaluate.
    /// </summary>
    /// <remarks>The collection of objects all implement the IEvaluate interface. Therefore, the collection 
    /// of objects must also inherit one of the abstract classes Scope or LogicalConnectiveCollection.
    /// </remarks>
    [Serializable]
    [XmlSchemaProvider(null, IsAny = true)]
    public class LogicalOrCollection : LogicalConnectiveCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalOrCollection"/> class.
        /// </summary>
        public LogicalOrCollection()
            : base()
        {
        }

        new public static LogicalConnectiveCollection Load(XmlReader reader)
        {
            LogicalOrCollection loc = new LogicalOrCollection();
            loc.ReadXml(reader);

            return loc;
        }

        /// <summary>
        /// Evaluates a set of claims.
        /// </summary>
        /// <param name="claimSet">The set of claims to be evaluated.</param>
        /// <returns>True, if the evaluation is true; otherwise false.</returns>
        public override bool Evaluate(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            bool eval = false;

            foreach (Term item in this)
            {
                eval = item.Evaluate(claims);
                if (this.Evaluates)
                {
                    if (eval)
                    {
                        return true;
                    }
                }
                else
                {
                    if (eval)
                    {
                        return false;
                    }
                }
            }

            return !this.Evaluates;
        }

      

        /// <summary>
        /// Reads the Xml of a logical OR.
        /// </summary>
        /// <param name="reader">An XmlReader for a logical OR.</param>
        public override void ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            reader.MoveToRequiredStartElement(AuthorizationConstants.Elements.LogicalOr);

            string evaluates = reader.GetOptionalAttribute(AuthorizationConstants.Attributes.Evaluates);
            string termId = reader.GetOptionalAttribute(AuthorizationConstants.Attributes.TermId);

            if (!string.IsNullOrEmpty(termId))
            {
                this.TermId = new Uri(termId);
            }

            if (!string.IsNullOrEmpty(evaluates))
            {
                this.Evaluates = XmlConvert.ToBoolean(evaluates);
            }

            while (reader.Read())
            {
                if (reader.IsRequiredStartElement(AuthorizationConstants.Elements.LogicalAnd))
                {
                    this.Add(LogicalAndCollection.Load(reader));
                }

                if (reader.IsRequiredStartElement(AuthorizationConstants.Elements.LogicalOr))
                {
                    this.Add(LogicalOrCollection.Load(reader));
                }

                if (reader.IsRequiredStartElement(AuthorizationConstants.Elements.Rule))
                {
                    this.Add(Rule.Load(reader));
                }               

                if (reader.IsRequiredEndElement(AuthorizationConstants.Elements.LogicalOr))
                {
                    break;
                }
            }

            reader.Read();
        }

        /// <summary>
        /// Writes the Xml of a logical OR.
        /// </summary>
        /// <param name="writer">An XmlWriter for a logical OR.</param>
        public override void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteStartElement(AuthorizationConstants.Elements.LogicalOr, AuthorizationConstants.Namespaces.Xmlns);

            if (this.TermId != null)
            {
                writer.WriteAttributeString(AuthorizationConstants.Attributes.TermId, this.TermId.ToString());
            }

            writer.WriteAttributeString(AuthorizationConstants.Attributes.Evaluates, XmlConvert.ToString(this.Evaluates));

            foreach (Term eval in this)
            {
                eval.WriteXml(writer);
            }

            writer.WriteEndElement();            
        }
    }
}
