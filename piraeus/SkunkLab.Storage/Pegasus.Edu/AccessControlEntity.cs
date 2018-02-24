using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using System.Reflection;

namespace Pegasus.Edu
{
    public class AccessControlEntity : TableEntity
    {
        public AccessControlEntity()
        {
        }

        public AccessControlEntity(string resourceUriString, string policyUriString, string name, string school, string policy)
        {
            string resourceEncodedUri = Convert.ToBase64String(Encoding.UTF8.GetBytes(resourceUriString)).Replace("/", "_");
            string policyEncodedUri = Convert.ToBase64String(Encoding.UTF8.GetBytes(policyUriString)).Replace("/", "_");
            this.ResourceUriString = resourceEncodedUri;
            this.PolicyUriString = policyEncodedUri;
            this.Name = name;
            this.School = school;
            this.Policy = policy;
        }

        public string ResourceUriString
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        public string Name { get; set; }

        public string PolicyUriString
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public string School { get; set; }

        public string Policy { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            foreach (var item in properties)
            {
                if(item.Key == "ResourceUriString")
                {
                    string value = Encoding.UTF8.GetString(Convert.FromBase64String(item.Value.StringValue.Replace("_", "/")));
                    this.ResourceUriString = value;                    
                }

                if (item.Key == "PolicyUriString")
                {
                    string value = Encoding.UTF8.GetString(Convert.FromBase64String(item.Value.StringValue.Replace("_", "/")));
                    this.ResourceUriString = value;
                }
            }

        }


        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {


            return base.WriteEntity(operationContext);
        }
    }
}
