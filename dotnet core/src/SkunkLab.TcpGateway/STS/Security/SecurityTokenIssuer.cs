using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STS.Security
{
    public class SecurityTokenIssuer
    {
        public SecurityTokenIssuer(string connectionString)
        {
            CloudStorageAccount acct = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = acct.CreateCloudTableClient();
            table = tableClient.GetTableReference("rtu");
            
        }


        private string nameClaimType;
        private string roleClaimType;
        private CloudTable table;


        public async string IssueAsync(string luss)
        {

            LussEntity entity = await GetTableEntity(luss);
            if(entity == null || entity.CompleteTimestamp.HasValue)
            {
                return null;
            }

            //create publish CAPL policy
            //create subscribe CAPL policy
            //create Resource Metadata "into RTU"
            //create Resource Metadata "out of RTU"
            //update the RTU-MAP
            //create the security token
            //return the security token
        }

        public async Task<string> Reissue(string securityToken)
        {
            //validate the security token
            //get the claims from the security token
            //validate the token was issued 'completed'
            //reissue the security token
            //return security token
        }



        private async Task<LussEntity> GetTableEntity(string luss)
        {
            TableQuery<LussEntity> query = new TableQuery<LussEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, luss));
            TableQuerySegment<LussEntity> segment = await table.ExecuteQuerySegmentedAsync<LussEntity>(query, new TableContinuationToken());

            if (segment == null || segment.Results.Count == 0)
            {
                return null;
            }

            return segment.ToList()[0];
        }

        //private async Task CreateCaplPolicyAsync(AuthorizationPolicy policy)
        //{

        //}

        //private async Task CreateResourceMetadata(ResourceMetadata metadata)
        //{

        //}

        
        


    }
}
