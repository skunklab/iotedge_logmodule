/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Authorization
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Capl.Authorization.Transforms;

    /// <summary>
    /// The base class for an authorization policy.
    /// </summary>
    [Serializable]
    [XmlSchemaProvider("GetSchema", IsAny = false)]
    [KnownType(typeof(AuthorizationPolicy))]
    public abstract class AuthorizationPolicyBase : IXmlSerializable
    {
        /// <summary>
        /// Gets or sets a transform collection.
        /// </summary>
        public abstract TransformCollection Transforms { get; internal set; }

        /// <summary>
        /// Gets or sets an evaluation expression.
        /// </summary>
        public abstract Term Expression { get; set; }

        /// <summary>
        /// Provides a schema for an authorization policy.
        /// </summary>
        /// <param name="schemaSet">A schema set to populate.</param>
        /// <returns>Qualified name of an authorization policy type for a schema.</returns>
        public static XmlQualifiedName GetSchema(XmlSchemaSet schemaSet)
        {
            if (schemaSet == null)
            {
                throw new ArgumentNullException("schemaSet");
            }
            
            using (StringReader reader = new StringReader(Capl.Properties.Resources.AuthorizationPolicySchema))
            {
                XmlSchema schema = XmlSchema.Read(reader, null);
                schemaSet.Add(schema);
            }

            return new XmlQualifiedName("AuthorizationPolicyType", AuthorizationConstants.Namespaces.Xmlns);
        }

        #region IXmlSerializable Members

        /// <summary>
        /// Provides a schema for an authorization policy.
        /// </summary>
        /// <returns>Schema for an authorization policy.</returns>
        /// <remarks>The methods always return null; the schema is provided by an XmlSchemaProvider.</remarks>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads the Xml of an authorization policy.
        /// </summary>
        /// <param name="reader">An XmlReader for the authorization policy.</param>
        public abstract void ReadXml(XmlReader reader);

        /// <summary>
        /// Writes the Xml of an authorization policy.
        /// </summary>
        /// <param name="writer">An XmlWriter for the authorization policy.</param>
        public abstract void WriteXml(XmlWriter writer);

        #endregion        
    }
}
