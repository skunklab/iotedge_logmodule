/*
Claims Authorization Policy Langugage SDK ver. 1.0 
Copyright (c) Matt Long labskunk@gmail.com 
All rights reserved. 
MIT License
*/

namespace Capl.Issuance
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Security.Claims;
    using Capl.Authorization;
    using Capl.Authorization.Transforms;
    using System.Xml.Serialization;

    [Serializable]
    [XmlSchemaProvider(null, IsAny = true)]
    public class IssuePolicy : IssuePolicyBase
    {
        public IssuePolicy()
        {
            this._transforms = new TransformCollection();    
        }

        private string _policyId;
        private TransformCollection _transforms;
        private IssueMode _mode;

        public string PolicyId
        {
            get { return this._policyId; }
            set { this._policyId = value; }
        }

        public IssueMode Mode
        {
            get { return this._mode; }
            set { this._mode = value; }
        }

        public TransformCollection Transforms
        {
            get { return this._transforms; }
        }

        public static IssuePolicy Load(XmlReader reader)
        {
            IssuePolicy policy = new IssuePolicy();
            policy.ReadXml(reader);

            return policy;
        }

        public ClaimsIdentity Issue(ClaimsIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            return new ClaimsIdentity(Issue(identity.Claims));
        }

        public IEnumerable<Claim> Issue(IEnumerable<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims);
            List<Claim> clone = new List<Claim>(identity.Claims);

            List<Claim> copyList = new List<Claim>();
            foreach (Claim claim in clone)
            {
                copyList.Add(claim);
            }

            IEnumerable<Claim> inputClaims = new ClaimsIdentity(copyList).Claims;

            List<ICollection<Claim>> list = new List<ICollection<Claim>>();
            
            foreach (ClaimTransform transform in this._transforms)
            {
                clone = new List<Claim>(transform.TransformClaims(clone.ToArray()));
            }
            
            

            if (this._mode == IssueMode.Unique)
            {
                foreach (Claim c in inputClaims)
                {
                    ICollection<Claim> claimSet = clone.FindAll(delegate(Claim claim)
                    {
                        return (c.Type == claim.Type && c.Value == claim.Value && c.Issuer == claim.Issuer);
                    });

                    if (claimSet.Count > 0)
                    {
                        list.Add(claimSet);
                    }
                }

                foreach (ICollection<Claim> claimCollection in list)
                {
                    foreach (Claim c in claimCollection)
                    {
                        clone.Remove(c);
                    }
                }                
            }

            return clone;
        }

        public override void ReadXml(XmlReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            
            reader.MoveToRequiredStartElement(IssueConstants.Elements.IssuePolicy, IssueConstants.Namespaces.Xmlns);
            this._policyId = reader.GetOptionalAttribute(IssueConstants.Attributes.PolicyId);
            string mode = reader.GetOptionalAttribute(IssueConstants.Attributes.Mode);

            if (mode == IssueConstants.IssueModes.Aggregate)
            {
                this._mode = IssueMode.Aggregate;
            }
            else if (mode == IssueConstants.IssueModes.Unique)
            {
                this._mode = IssueMode.Unique;
            }
            else if (mode == null)
            {
                this._mode = IssueMode.Aggregate;
            }
            else
            {
                throw new IssueModeNotRecognizedException("Issue mode is not recognized.");
            }

            while (reader.Read())
            {
                if(reader.IsRequiredStartElement(AuthorizationConstants.Elements.Transforms))
                {
                    this._transforms = TransformCollection.Load(reader);
                }

                if (reader.IsRequiredEndElement(IssueConstants.Elements.IssuePolicy, IssueConstants.Namespaces.Xmlns))
                {
                    break;
                }
            }

            reader.Read();            
        }


        public override void WriteXml(XmlWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            
            writer.WriteStartElement(IssueConstants.Elements.IssuePolicy, IssueConstants.Namespaces.Xmlns);

            if (this._mode == IssueMode.Aggregate)
            {
                writer.WriteAttributeString(IssueConstants.Attributes.Mode, IssueConstants.IssueModes.Aggregate);
            }
            else
            {
                writer.WriteAttributeString(IssueConstants.Attributes.Mode, IssueConstants.IssueModes.Unique);
            }

            if (this._policyId != null)
            {
                writer.WriteAttributeString(AuthorizationConstants.Attributes.PolicyId, this._policyId);
            }

            this._transforms.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
