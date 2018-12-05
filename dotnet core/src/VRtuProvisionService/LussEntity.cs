using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace VRtuProvisionService
{
    public class LussEntity : TableEntity
    {
        public LussEntity()
        {

        }

        public LussEntity(string moduleIdentity, string luss)
        {

        }

        public static async Task<LussEntity> LoadAsync(string luss, string tableName, string connectionString)
        {
            CloudStorageAccount acct = CloudStorageAccount.Parse(connectionString);
            CloudTableClient client = acct.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            TableQuery<LussEntity> query = new TableQuery<LussEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, luss));

            TableQuerySegment<LussEntity> segment = await table.ExecuteQuerySegmentedAsync<LussEntity>(query, new TableContinuationToken());

            if (segment == null || segment.Results.Count == 0 || segment.Results.Count > 1)
            {
                return null;
            }
            else
            {
                return segment.Results[0];
            }

        }

        /// <summary>
        /// The Limited Use Shared Secret (LUSS)
        /// </summary>
        public string Luss
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        /// <summary>
        /// The name of the virtual RTU, e.g., alberta
        /// </summary>
        public string VirtualRtuId
        {
            get { return RowKey; }
            set { RowKey = value.ToLowerInvariant(); }
        }

        /// <summary>
        /// The Unit ID of the physical RTU.
        /// </summary>
        /// <remarks>The Unit ID is used in the ModBus-TCP MBAP header.</remarks>
        public int UnitId { get; set; }

        /// <summary>
        /// The IoTHub module ID
        /// </summary>
        /// <remarks>This is for query information of deployments</remarks>
        public string ModuleId { get; set; }

        /// <summary>
        /// The IoTHub device ID
        /// </summary>
        /// <remarks>This is for query information of deployments</remarks>
        public string DeviceId { get; set; }

        /// <summary>
        /// The timestamp when the LUSS was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// The timestamp when the LUSS will expire if not used.
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// The timestamp when the LUSS was received from the module.
        /// </summary>
        public DateTime? Access { get; set; }

        /// <summary>
        /// Determines if the function returned successfully to issue token parameters.
        /// </summary>
        /// <remarks>If the Success property is not null, then the LUSS cannot be reused.
        /// The LUSS is intended as one time use.</remarks>
        public bool? Success { get; set; }

        /// <summary>
        /// The timestamp when a security token was reissued.
        /// </summary>
        /// <remarks>Feature TBD.</remarks>
        public DateTime? Reissued { get; set; }        

        /// <summary>
        /// Updates the RTU Token table in Azure.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public async Task UpdateAsync(string connectionString)
        {
            CloudStorageAccount acct = CloudStorageAccount.Parse(connectionString);
            CloudTableClient client = acct.CreateCloudTableClient();
            CloudTable table = client.GetTableReference("rtutokens");

            TableOperation operation = TableOperation.InsertOrReplace(this);
            await table.ExecuteAsync(operation);
        }


    }


}
