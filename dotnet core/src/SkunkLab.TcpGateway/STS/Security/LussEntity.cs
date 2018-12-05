using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STS.Security
{
    public class LussEntity : TableEntity
    {
        public LussEntity()
        {

        }

        public LussEntity(string moduleIdentity, string luss)
        {

        }

        public ushort UnitId { get; set; }
        public string ModuleIdentity { get; set; }
        public string Luss { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime? IssueTimestamp { get; set; }
        public DateTime? CompleteTimestamp { get; set; }
        public DateTime? ReissueTimestamp { get; set; }
        public bool? Expired { get; set; }



    }
}
