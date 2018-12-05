using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class TableStorage
    {
        private HashSet<string> tableNames;
        protected TableStorage(string connectionString)
        {
            tableNames = new HashSet<string>();
            CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, Convert.ToBase64String(account.Credentials.ExportKey()));
            client = new CloudTableClient(account.TableStorageUri, credentials);
            client.DefaultRequestOptions.ServerTimeout = TimeSpan.FromMinutes(1.0);
            client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(10000), 8);
            client.DefaultRequestOptions.MaximumExecutionTime = TimeSpan.FromMinutes(3.0);

            if (bufferManager != null)
            {
                client.BufferManager = bufferManager;
            }
        }
        
        private static SkunkLabBufferManager bufferManager;

        public static TableStorage CreateSingleton(string connectionString)
        {
            if(instance == null)
            {
                instance = new TableStorage(connectionString);
            }

            return instance;
        }

        public static TableStorage CreateSingleton(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(connectionString);
        }
        
        public static TableStorage New(string connectionString)
        {
            return new TableStorage(connectionString);
        }
        
        public static TableStorage New(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);            
            return new TableStorage(connectionString);
        }


        private static TableStorage instance;
        private CloudTableClient client;

        #region Write Table
                
        public async Task WriteAsync(string tableName, ITableEntity entity)
        {
            if(entity == null)
            {
                Trace.TraceWarning("Table {0} entity is null", tableName);
                return;
            }

            try
            {
                CloudTable table = client.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();
                TableOperation operation = TableOperation.InsertOrReplace(entity);
                await table.ExecuteAsync(operation);                
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Table {0} failed write.", tableName);
                Trace.TraceError("Table {0} write error {1}", tableName, ex.Message);
            }
        }

        #endregion

        #region Read Table       

        public async Task<List<T>> ReadAsync<T>(string tableName) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            var query = new TableQuery<T>();
            TableQuerySegment<T> segment = null;
            segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken());

            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<T>> ReadAsync<T>(string tableName, string partitionKey = null, string rowKey = null) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            TableQuery<T> query = null;

            if (!string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                string q1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
                string q2 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
                query = new TableQuery<T>().Where(TableQuery.CombineFilters(q1, TableOperators.And, q2));
            }
            else if (!string.IsNullOrEmpty(partitionKey) && string.IsNullOrEmpty(rowKey))
            {
                query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }
            else if (string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
            }
            else
            {
                query = new TableQuery<T>();
            }

            TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken()); 
          
            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<T>> ReadAsync<T>(string tableName, string fieldName, string operation, string value) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition(fieldName, operation, value));
            TableQuerySegment<T> segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken());
            
            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        #endregion




    }
}
