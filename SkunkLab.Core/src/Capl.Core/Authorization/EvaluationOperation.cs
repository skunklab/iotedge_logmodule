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

    /// <summary>
    /// An abstract operation that performs an evaluation.
    /// </summary>
    [Serializable]
    [XmlSchemaProvider(null, IsAny = true)]
    public class EvaluationOperation : IXmlSerializable
    {
        /// <summary>
        /// The URI that identifies an operation.
        /// </summary>
        private Uri _operationType;

        /// <summary>
        /// The claim value defined by the operation.
        /// </summary>
        private string _claimValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationOperation"/> class.
        /// </summary>
        public EvaluationOperation()
        {
        }

        // <summary>
        /// Initializes a new instance of the <see cref="EvaluationOperation"/> class.
        /// </summary>
        /// <param name="operationType">The URI that identifies the operation.</param>
        /// <param name="claimValue">The claim value defined by the operation.</param>
        public EvaluationOperation(Uri operationType, string claimValue)
        {
            this._operationType = operationType;
            this._claimValue = claimValue;
        }

        /// <summary>
        /// Gets or sets the URI that identifies the operation.
        /// </summary>
        public Uri Type
        {
            get { return this._operationType; }
            set { this._operationType = value; }
        }

        /// <summary>
        /// Gets or sets the claim value defined by the operation.
        /// </summary>
        /// <remarks>If the claim value is null, it implies a unary operation.</remarks>
        public string ClaimValue
        {
            get { return this._claimValue; }
            set { this._claimValue = value; }
        }

        public static EvaluationOperation Load(XmlReader reader)
        {
            EvaluationOperation evalOperation = new EvaluationOperation();
            evalOperation.ReadXml(reader);

            return evalOperation;
        }


        #region IXmlSerializable Members

        /// <summary>
        /// Provides a schema for an operation.
        /// </summary>
        /// <returns>Schema for an operation.</returns>
        /// <remarks>The methods always return null; the schema is provided by an XmlSchemaProvider.</remarks>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads the Xml of an operation.
        /// </summary>
        /// <param name="reader">An XmlReader for the operation.</param>
        public void ReadXml(XmlReader reader)
        {   
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            reader.MoveToRequiredStartElement(AuthorizationConstants.Elements.Operation);
            this._operationType = new Uri(reader.GetRequiredAttribute(AuthorizationConstants.Attributes.Type));
            this._claimValue = reader.GetElementValue(AuthorizationConstants.Elements.Operation);
            
            if (!reader.IsRequiredEndElement(AuthorizationConstants.Elements.Operation))
            {
                throw new SerializationException("Unexpected element " + reader.LocalName);
            }            
        }

        /// <summary>
        /// Writes the Xml of an operation.
        /// </summary>
        /// <param name="writer">An XmlWriter for the operation.</param>
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteStartElement(AuthorizationConstants.Elements.Operation, AuthorizationConstants.Namespaces.Xmlns);
            writer.WriteAttributeString(AuthorizationConstants.Attributes.Type, this._operationType.ToString());            
            writer.WriteString(this._claimValue);
            writer.WriteEndElement();            
        }

        #endregion
    }
}
