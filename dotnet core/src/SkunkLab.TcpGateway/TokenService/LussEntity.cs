using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TokenService
{
    public class LussEntity : TableEntity
    {
        public LussEntity()
        {

        }

        public LussEntity(string moduleIdentity, string luss)
        {

        }

        public static async Task<LussEntity> LoadAsync(string luss, string connectionString)
        {
            CloudStorageAccount acct = CloudStorageAccount.Parse(connectionString);
            CloudTableClient client  = acct.CreateCloudTableClient();
            CloudTable table = client.GetTableReference("rtutokens");
            
            TableQuery<LussEntity> query = new TableQuery<LussEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, luss));

            TableQuerySegment<LussEntity> segment = await table.ExecuteQuerySegmentedAsync<LussEntity>(query, new TableContinuationToken());

            if(segment == null || segment.Results.Count == 0 || segment.Results.Count > 1)
            {
                return null;
            }
            else
            {
                return segment.Results[0];
            }


        }



        public string Luss
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }


        public string ModuleIdentity
        {
            get { return RowKey; }
            set { RowKey = value; }
        }
        public ushort UnitId { get; set; }

        public string GroupId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Issued { get; set; }
        public DateTime? Completed { get; set; }
        public DateTime? Reissued { get; set; }
        public bool? Expired { get; set; }



    }


}
