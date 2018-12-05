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
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// An interface used to evaluate a set of claims or a collection of claims and set the truthful evaluation
    ///  for both.
    /// </summary>
    /// <remarks>The abstract LogicalConnectiveCollection implements this interface.</remarks>
    [Serializable]
    public abstract class Term : IXmlSerializable
    {
        /// <summary>
        /// Get or sets the truthful evaluation.
        /// </summary>
        public abstract bool Evaluates { get; set; }
        public abstract Uri TermId { get; set; }
        public abstract bool Evaluate(IEnumerable<Claim> claims);

        public static Term Load(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            Term evalExp = null;

            reader.MoveToStartElement();

            if(reader.IsRequiredStartElement(AuthorizationConstants.Elements.Rule))
            {
                Rule rule = new Rule();
                rule.ReadXml(reader);
                evalExp = rule;
            }

            if(reader.IsRequiredStartElement(AuthorizationConstants.Elements.LogicalAnd))
            {
                LogicalAndCollection logicalAnd = new LogicalAndCollection();
                logicalAnd.ReadXml(reader);
                evalExp = logicalAnd;
            }


            if(reader.IsRequiredStartElement(AuthorizationConstants.Elements.LogicalOr))
            {
                LogicalOrCollection logicalOr = new LogicalOrCollection();
                logicalOr.ReadXml(reader);
                evalExp = logicalOr;
            }

            if (evalExp != null)
            {
                return evalExp;
            }
            else
            {
                throw new SerializationException("Invalid evaluation expression element.");
            }
        }

        public virtual XmlSchema GetSchema()
        {
            return null;
        }

        public abstract void ReadXml(XmlReader reader);

        public abstract void WriteXml(XmlWriter writer);
    }
}
