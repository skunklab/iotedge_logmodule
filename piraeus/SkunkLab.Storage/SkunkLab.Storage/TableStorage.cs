using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class TableStorage
    {
        protected TableStorage(string connectionString)
        {
            try
            {
                CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
                StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, account.Credentials.ExportKey());
                client = new CloudTableClient(account.TableStorageUri, credentials);

                if (bufferManager != null)
                {
                    client.BufferManager = bufferManager;
                }
            }
            catch(Exception ex)
            {
                TraceManager.WriteError(101, ex.Message);
            }
        }

        protected TableStorage(string vault, string clientId, string clientSecret, string keyName)
        {
            try
            {
                keyVault = new Vault(vault, clientId, clientSecret, keyName);
                CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(keyVault.Key);
                StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, account.Credentials.ExportKey());
                client = new CloudTableClient(account.TableStorageUri, credentials);

                if (bufferManager != null)
                {
                    client.BufferManager = bufferManager;
                }
            }
            catch(Exception ex)
            {
                TraceManager.WriteError(102, ex.Message);
                throw;
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

        public static TableStorage CreateSingleton(string vault, string clientId, string clientSecret, string keyName)
        {
            if(instance == null)
            {
                instance = new TableStorage(vault, clientId, clientSecret, keyName);
            }

            return instance;
        }

        public static TableStorage CreateSingleton(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(connectionString);
        }

        public static TableStorage CreateSingleton(string vault, string clientId, string clientSecret, string keyName, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(vault, clientId, clientSecret, keyName);

        }

        public static TableStorage New(string connectionString)
        {
            return new TableStorage(connectionString);
        }

        public static TableStorage New(string vault, string clientId, string clientSecret, string keyName)
        {
            return new TableStorage(vault, clientId, clientSecret, keyName);
        }

        public static TableStorage New(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);            
            return new TableStorage(connectionString);
        }

        public static TableStorage New(string vault, string clientId, string clientSecret, string keyName, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return new TableStorage(vault, clientId, clientSecret, keyName);
        }

        private static TableStorage instance;
        private CloudTableClient client;
        private Vault keyVault;

        #region Write Table

        public void Write(string tableName, ITableEntity entity, string encryptKeyName = null)
        {
            CloudTable table = client.GetTableReference(tableName);
            table.CreateIfNotExists();
            TableOperation operation = TableOperation.InsertOrReplace(entity);

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                table.Execute(operation);
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                table.Execute(operation, options);
            }
        }

        public async Task WriteAsync(string tableName, ITableEntity entity, string encryptKeyName = null)
        {
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            TableOperation operation = TableOperation.InsertOrReplace(entity);

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                await table.ExecuteAsync(operation);
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                await table.ExecuteAsync(operation, options, null);
            }
        }

        #endregion

        #region Read Table
        public List<T> Read<T>(string tableName, string encryptKeyName = null) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            table.CreateIfNotExists();
            var query = new TableQuery<T>();
            TableQuerySegment<T> segment = null;

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                segment = table.ExecuteQuerySegmented<T>(query, new TableContinuationToken());
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                segment = table.ExecuteQuerySegmented<T>(query, new TableContinuationToken(), options);
            }
            
            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        public List<T> Read<T>(string tableName, string partitionKey = null, string rowKey = null, string encryptKeyName = null) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            table.CreateIfNotExists();
            TableQuery<T> query = null;

            if(!string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                string q1 = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
                string q2 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
                query = new TableQuery<T>().Where(TableQuery.CombineFilters(q1, TableOperators.And, q2));
            }
            else if(!string.IsNullOrEmpty(partitionKey) && string.IsNullOrEmpty(rowKey))
            {
                query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }
            else if(string.IsNullOrEmpty(partitionKey) && !string.IsNullOrEmpty(rowKey))
            {
                query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey));
            }
            else
            {
                query = new TableQuery<T>();
            }

            TableQuerySegment<T> segment = null;

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                segment = table.ExecuteQuerySegmented<T>(query, new TableContinuationToken());
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                segment = table.ExecuteQuerySegmented<T>(query, new TableContinuationToken(), options);
            }

            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        public List<T> Read<T>(string tableName, string fieldName, string operation, string value, string encryptKeyName = null) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            table.CreateIfNotExists();
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition(fieldName, operation, value));
            TableQuerySegment<T> segment = null;

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                segment = table.ExecuteQuerySegmented<T>(query, new TableContinuationToken());
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                segment = table.ExecuteQuerySegmented<T>(query, new TableContinuationToken(), options);
            }

            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<T>> ReadAsync<T>(string tableName, string encryptKeyName = null) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            var query = new TableQuery<T>();
            TableQuerySegment<T> segment = null;

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken());
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken(), options, null);
            }

            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<T>> ReadAsync<T>(string tableName, string partitionKey = null, string rowKey = null, string encryptKeyName = null) where T : ITableEntity, new()
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

            TableQuerySegment<T> segment = null;

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken());
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken(), options, null);
            }

            if (!(segment == null || segment.Results.Count == 0))
            {
                return segment.ToList();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<T>> ReadAsync<T>(string tableName, string fieldName, string operation, string value, string encryptKeyName = null) where T : ITableEntity, new()
        {
            CloudTable table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition(fieldName, operation, value));
            TableQuerySegment<T> segment = null;

            if (!string.IsNullOrEmpty(encryptKeyName))
            {
                segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken());
            }
            else
            {
                TableRequestOptions options = keyVault.GetEncryptionTableOptions(encryptKeyName);
                segment = await table.ExecuteQuerySegmentedAsync<T>(query, new TableContinuationToken(), options, null);
            }

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
