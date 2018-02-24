using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pegasus.Edu
{
    public class ProjectEntity : TableEntity
    {
        public ProjectEntity()
        {
        }

        public ProjectEntity(int code, string projectName)
        {
            this.PartitionKey = code.ToString();
            this.Name = projectName;
        }

        public ProjectEntity(string code, string projectName)
        {
            this.PartitionKey = code;
            this.Name = projectName;
        }

        public string Code
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        public string Name
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public string Country { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string School { get; set; }

        public string Contact { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}
