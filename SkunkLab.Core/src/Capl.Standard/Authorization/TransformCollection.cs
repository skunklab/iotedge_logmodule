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
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Security.Claims;
    using Capl.Authorization;

    [Serializable]
    [XmlSchemaProvider(null, IsAny = true)]
    public class TransformCollection : TransformBase, IList<Transform>, IXmlSerializable
    {
        public TransformCollection()
        {
            transforms = new List<Transform>();
        }

        private List<Transform> transforms;

        public static TransformCollection Load(XmlReader reader)
        {
            TransformCollection transforms = new TransformCollection();
            transforms.ReadXml(reader);

            return transforms;
        }

        #region IList<ScopeTransform> Members

        public int IndexOf(Transform item)
        {
            return transforms.IndexOf(item);
        }

        public void Insert(int index, Transform item)
        {
            transforms.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            transforms.RemoveAt(index);
        }

        public Transform this[int index]
        {
            get { return transforms[index]; }
            set { transforms[index] = value; }
        }

        #endregion

        #region ICollection<ScopeTransform> Members

        public void Add(Transform item)
        {
            transforms.Add(item);
        }

        public void Clear()
        {
            transforms.Clear();
        }

        public bool Contains(Transform item)
        {
            return transforms.Contains(item);
        }

        public void CopyTo(Transform[] array, int arrayIndex)
        {
            transforms.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return transforms.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Transform item)
        {
            return transforms.Remove(item);
        }

        #endregion

        #region IEnumerable<ScopeTransform> Members

        public IEnumerator<Transform> GetEnumerator()
        {
            return transforms.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        #endregion

        #region ITransform Members

        public override IEnumerable<Claim> TransformClaims(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            foreach (Transform transform in this)
            {
                claims = transform.TransformClaims(claims);
            }
            

            return claims;
        }

        #endregion

        #region IXmlSerializable Members


        public override void ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            
            reader.MoveToRequiredStartElement(AuthorizationConstants.Elements.Transforms);

            while (reader.Read())
            {
                if (reader.IsRequiredStartElement(AuthorizationConstants.Elements.Transform))
                {
                    this.Add(ClaimTransform.Load(reader));
                }

                if (reader.IsRequiredEndElement(AuthorizationConstants.Elements.Transforms))
                {
                    break;
                }
            }            
        }

        public override void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (this.Count == 0)
            {
                return;
            }
            
            writer.WriteStartElement(AuthorizationConstants.Elements.Transforms, AuthorizationConstants.Namespaces.Xmlns);

            foreach (ClaimTransform transform in this)
            {
                transform.WriteXml(writer);
            }

            writer.WriteEndElement();            
        }

        #endregion
    }
}
