/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public class Match : IXmlSerializable
    {
        public Match()
        {

        }

        public Match(Uri matchExpressionUri, string claimType)
            : this(matchExpressionUri, claimType, true, null)
        {

        }

        public Match(Uri matchExpressionUri, string claimType, bool required)
            : this(matchExpressionUri, claimType, required, null)
        {

        }

        public Match(Uri matchExpressionUri, string claimType, bool required, string value)
        {
            Type = matchExpressionUri;
            ClaimType = claimType;
            Required = required;
            Value = value;
        }
        /// <summary>
        /// Gets or set the type of match expression
        /// </summary>
        public Uri Type { get; set; }

        /// <summary>
        /// Gets or sets a value for a claim type.
        /// </summary>
        public string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets a value for claim.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether matching is required for evaluation.
        /// </summary>
        public bool Required { get; set; }


        public static Match Load(XmlReader reader)
        {
            Match match = new Match();
            match.ReadXml(reader);
            return match;           
        }
                
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            reader.MoveToRequiredStartElement(AuthorizationConstants.Elements.Match);
            this.ClaimType = reader.GetOptionalAttribute(AuthorizationConstants.Attributes.ClaimType);
            this.Type = new Uri(reader.GetRequiredAttribute(AuthorizationConstants.Attributes.Type));
            string required = reader.GetOptionalAttribute(AuthorizationConstants.Attributes.Required);

            if (string.IsNullOrEmpty(required))
            {
                this.Required = true;
            }
            else
            {
                this.Required = XmlConvert.ToBoolean(required);
            }

            this.Value = reader.GetElementValue(AuthorizationConstants.Elements.Match);

            if (!reader.IsRequiredEndElement(AuthorizationConstants.Elements.Match))
            {
                throw new SerializationException(String.Format("Unexpected element {0}", reader.LocalName));
            }
            
        }

        public void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteStartElement(AuthorizationConstants.Elements.Match, AuthorizationConstants.Namespaces.Xmlns);

            writer.WriteAttributeString(AuthorizationConstants.Attributes.Type, this.Type.ToString());
            writer.WriteAttributeString(AuthorizationConstants.Attributes.ClaimType, this.ClaimType);
            writer.WriteAttributeString(AuthorizationConstants.Attributes.Required, XmlConvert.ToString(this.Required));

            if (!string.IsNullOrEmpty(this.Value))
            {
                writer.WriteString(this.Value);
            }

            writer.WriteEndElement();          
        }
    }
}
