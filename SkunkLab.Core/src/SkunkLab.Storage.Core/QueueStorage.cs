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
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, Convert.ToBase64String(account.Credentials.ExportKey()));
     
            client = new CloudQueueClient(account.QueueStorageUri, credentials);
            
            container = new Dictionary<string, CloudQueue>();

            if(bufferManager != null)
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

        public static QueueStorage CreateSingleton(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(connectionString);
        }


        public static QueueStorage New(string connectionString)
        {
            return new QueueStorage(connectionString);
        }

      
        public static QueueStorage New(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return new QueueStorage(connectionString);
        }


        private static QueueStorage instance;
        private CloudQueueClient client;
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

        public async Task CreateQueueAsync(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);
            await queue.CreateIfNotExistsAsync();            
        }

        public async Task DeleteQueueAsync(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);
            await queue.DeleteIfExistsAsync();
        }


        public async Task ClearQueueAsync(string queueName)
        {
            CloudQueue queue = GetQueue(queueName);
            await queue.ClearAsync();
        }


        public async Task EnqueueAsync(string queueName, byte[] source, TimeSpan? ttl = null, TimeSpan? initialVisibilityDelay = null)
        {
            
            CloudQueue queue = GetQueue(queueName);
            
            if(! await queue.ExistsAsync())
            {
                throw new InvalidOperationException("Cloud queue does not exist.");
            }

            CloudQueueMessage message = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(source);
            await queue.AddMessageAsync(message, ttl, initialVisibilityDelay, new QueueRequestOptions(), null);
        }

        
        public async Task<List<CloudQueueMessage>> DequeueAsync(string queueName, int? numberOfMessages)
        {
            CloudQueue queue = GetQueue(queueName);

            if (! await queue.ExistsAsync())
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
                CloudQueueMessage message = await queue.GetMessageAsync();
                if(message != null)
                {
                    list.Add(message);
                }
            }
            else
            {
                list = new List<CloudQueueMessage>(await queue.GetMessagesAsync(numberOfMessages.Value));
            }

            return list;
        }

        
    }
}
