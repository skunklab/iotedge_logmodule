using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class QueueStorage
    {
        protected QueueStorage(string connectionString)
        {
            CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, account.Credentials.ExportKey());
            client = new CloudQueueClient(account.QueueStorageUri, credentials);
            container = new Dictionary<string, CloudQueue>();

            if(bufferManager != null)
            {
                client.BufferManager = bufferManager;
            }
        }

        protected QueueStorage(string vault, string clientId, string clientSecret, string keyName)
        {
            keyVault = new Vault(vault, clientId, clientSecret, keyName);
            CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(keyVault.Key);
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, account.Credentials.ExportKey());
            client = new CloudQueueClient(account.QueueStorageUri, credentials);
            container = new Dictionary<string, CloudQueue>();

            if (bufferManager != null)
            {
                client.BufferManager = bufferManager;
            }
            
        }

        private static SkunkLabBufferManager bufferManager;


        public static QueueStorage CreateSingleton(string connectionString)
        {
            if (instance == null)
            {
                instance = new QueueStorage(connectionString);
            }

            return instance;
        }

        public static QueueStorage CreateSingleton(string vault, string clientId, string clientSecret, string keyName)
        {
            if (instance == null)
            {
                instance = new QueueStorage(vault, clientId, clientSecret, keyName);
            }

            return instance;
        }

        public static QueueStorage CreateSingleton(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(connectionString);
        }

        public static QueueStorage CreateSingleton(string vault, string clientId, string clientSecret, string keyName, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(vault, clientId, clientSecret, keyName);
        }

        public static QueueStorage New(string connectionString)
        {
            return new QueueStorage(connectionString);
        }

        public static QueueStorage New(string vault, string clientId, string clientSecret, string keyName)
        {
            return new QueueStorage(vault, clientId, clientSecret, keyName);
        }

        public static QueueStorage New(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return new QueueStorage(connectionString);
        }

        public static QueueStorage New(string vault, string clientId, string clientSecret, string keyName, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return new QueueStorage(vault, clientId, clientSecret, keyName);

        }

        //, long maxBufferPoolSize, int defaultBufferSize


        private static QueueStorage instance;
        private CloudQueueClient client;
        private Vault keyVault;
        private Dictionary<string, CloudQueue> container;

        private CloudQueue GetQueue(string queueName)
        {
            CloudQueue queue = null;

            if (container.ContainsKey(queueName))
            {
                queue = container[queueName];
            }
            else
            {               
                queue = client.GetQueueReference(queueName);
                container.Add(queueName, queue);
            }
            return queue;
        }

        public void CreateQueue(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);
            queue.CreateIfNotExists();
            
        }

        public void DeleteQueue(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);
            
            if (queue.Exists())
            {
                queue.Delete();
            }
        }

        public async Task DeleteQueueAsync(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);

            if (await queue.ExistsAsync())
            {
                await queue.DeleteAsync();
            }
        }

        public void ClearQueue(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);
            
            if (queue.Exists())
            {
                queue.Clear();               
            }
        }

        public async Task ClearQueueAsync(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);

            if (await queue.ExistsAsync())
            {
                await queue.ClearAsync();
            }
        }

        public void Enqueue(string queueName, byte[] source, TimeSpan? ttl = null, TimeSpan? initialVisibilityDelay = null, string encryptKeyName = null)
        {
            CloudQueue queue = GetQueue(queueName);

            if(!queue.Exists())
            {
                throw new InvalidOperationException("Cloud queue does not exist.");
            }

            CloudQueueMessage message = new CloudQueueMessage(source);

            if (encryptKeyName == null)
            {                
                queue.AddMessage(message, ttl, initialVisibilityDelay);
            }
            else
            {
                QueueRequestOptions options = keyVault.GetEncryptionQueueOptions(encryptKeyName);
                queue.AddMessage(message, ttl, initialVisibilityDelay, options);
            }
        }

        public async Task EnqueueAsync(string queueName, byte[] source, TimeSpan? ttl = null, TimeSpan? initialVisibilityDelay = null, string encryptKeyName = null)
        {
            CloudQueue queue = GetQueue(queueName);

            if (!await queue.ExistsAsync())
            {
                throw new InvalidOperationException("Cloud queue does not exist.");
            }

            CloudQueueMessage message = new CloudQueueMessage(source);

            if (encryptKeyName == null)
            {
                await queue.AddMessageAsync(message, ttl, initialVisibilityDelay, null, null);
            }
            else
            {
                QueueRequestOptions options = keyVault.GetEncryptionQueueOptions(encryptKeyName);
                await queue.AddMessageAsync(message, ttl, initialVisibilityDelay, options, null);
            }

        }

        public List<CloudQueueMessage> Dequeue(string queueName, int? numberOfMessages, string encryptKeyName = null)
        {
            CloudQueue queue = GetQueue(queueName);

            if (!queue.Exists())
            {
                throw new InvalidOperationException("Cloud queue does not exist.");
            }

            int? approxMessageCount = queue.ApproximateMessageCount;


            List<CloudQueueMessage> list = approxMessageCount.HasValue ? new List<CloudQueueMessage>() : null;

            if(list == null)
            {
                return null;
            }

            if (!numberOfMessages.HasValue)
            {
                if (encryptKeyName == null)
                {
                    list.Add(queue.GetMessage());
                }
                else
                {
                    QueueRequestOptions options = keyVault.GetEncryptionQueueOptions(encryptKeyName);
                    list.Add(queue.GetMessage(null, options));
                }

            }
            else
            {
                if (encryptKeyName == null)
                {
                    list = new List<CloudQueueMessage>(queue.GetMessages(numberOfMessages.Value));
                }
                else
                {
                    QueueRequestOptions options = keyVault.GetEncryptionQueueOptions(encryptKeyName);
                    list = new List<CloudQueueMessage>(queue.GetMessages(numberOfMessages.Value, null, options));
                }
            }

            return list;
        }

        public async  Task<List<CloudQueueMessage>> DequeueAsync(string queueName, int? numberOfMessages, string encryptKeyName)
        {
            CloudQueue queue = GetQueue(queueName);

            if (!await queue.ExistsAsync())
            {
                throw new InvalidOperationException("Cloud queue does not exist.");
            }

            int? approxMessageCount = queue.ApproximateMessageCount;


            List<CloudQueueMessage> list = approxMessageCount.HasValue ? new List<CloudQueueMessage>() : null;

            if (list == null)
            {
                return null;
            }

            if (!numberOfMessages.HasValue)
            {
                if (encryptKeyName == null)
                {
                    list.Add(await queue.GetMessageAsync());
                }
                else
                {
                    QueueRequestOptions options = keyVault.GetEncryptionQueueOptions(encryptKeyName);
                    list.Add(await queue.GetMessageAsync(null, options, null));
                }
            }
            else
            {
                if (encryptKeyName == null)
                {
                    list = new List<CloudQueueMessage>(await queue.GetMessagesAsync(numberOfMessages.Value));
                }
                else
                {
                    QueueRequestOptions options = keyVault.GetEncryptionQueueOptions(encryptKeyName);
                    list = new List<CloudQueueMessage>(await queue.GetMessagesAsync(numberOfMessages.Value, null, options, null));
                }
            }

            return list;
        }
    }
}
