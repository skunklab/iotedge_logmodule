using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class BlobStorage
    {
        /// <summary>
        /// Sets the connection string for Blob storage via Key Vault
        /// </summary>
        /// <param name="vault">Name of Key Vault</param>
        /// <param name="clientId">Client ID for Key Vault access.</param>
        /// <param name="clientSecret">Client secret for Key Vault access.</param>
        /// <param name="keyName">Name of Key Vault key.</param>
        protected BlobStorage(string vault, string clientId, string clientSecret, string keyName)
        {
            keyVault = new Vault(vault, clientId, clientSecret, keyName);
            CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(keyVault.Key);
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, account.Credentials.ExportKey());
            client = new CloudBlobClient(account.BlobStorageUri, credentials);
            
            if(bufferManager != null)
            {
                client.BufferManager = bufferManager;
            }
        }

        protected BlobStorage(string connectionString)
        {
            CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, account.Credentials.ExportKey());
            client = new CloudBlobClient(account.BlobStorageUri, credentials);

            if (bufferManager != null)
            {
                client.BufferManager = bufferManager;
            }
        }

        private static SkunkLabBufferManager bufferManager;

        public static BlobStorage CreateSingleton(string connectionString)
        {
            if (instance == null)
            {
                instance = new BlobStorage(connectionString);
            }

            return instance;
        }

        public static BlobStorage CreateSingleton(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(connectionString);
        }

        public static BlobStorage CreateSingleton(string vault, string clientId, string clientSecret, string keyName)
        {
            if (instance == null)
            {
                instance = new BlobStorage(vault, clientId, clientSecret, keyName);
            }

            return instance;
        }

        public static BlobStorage CreateSingleton(string vault, string clientId, string clientSecret, string keyName, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return CreateSingleton(vault, clientId, clientSecret, keyName);
        }

        public static BlobStorage New(string connectionString)
        {
            return new BlobStorage(connectionString);
        }

        public static BlobStorage New(string vault, string clientId, string clientSecret, string keyName)
        {
            return new BlobStorage(vault, clientId, clientSecret, keyName);
        }

        public static BlobStorage New(string connectionString, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return new BlobStorage(connectionString);
        }

        public static BlobStorage New(string vault, string clientId, string clientSecret, string keyName, long maxBufferPoolSize, int defaultBufferSize)
        {
            BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
            bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            return new BlobStorage(vault, clientId, clientSecret, keyName);
        }

        private static BlobStorage instance;
        private CloudBlobClient client;
        private Vault keyVault;

        #region Blob Writers

        #region Block Blob Writers

        public void WriteBlockBlob(string containerName, string filename, Stream source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            CloudBlobContainer container = GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = contentType;

            Upload(blob, source, encryptKeyName);
        }

        public async Task WriteBlockBlobAsync(string containerName, string filename, Stream source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = contentType;

            await UploadAsync(blob, source, encryptKeyName);
        }
        public void WriteBlockBlob(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream stream = new MemoryStream(source))
            {
                WriteBlockBlob(containerName, filename, stream, contentType, encryptKeyName);
            }
        }

        public async Task WriteBlockBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream stream = new MemoryStream(source))
            {
                await WriteBlockBlobAsync(containerName, filename, stream, contentType, encryptKeyName);
            }
        }
        #endregion

        #region Page Blob Writers

        public void WritePageBlob(string containerName, string filename, Stream source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            
            CloudBlobContainer container = GetContainerReference(containerName);
            CloudPageBlob blob = container.GetPageBlobReference(filename);
            blob.Properties.ContentType = contentType;

            Upload(blob, source, encryptKeyName);
        }
        public async Task WritePageBlobAsync(string containerName, string filename, Stream source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudPageBlob blob = container.GetPageBlobReference(filename);
            blob.Properties.ContentType = contentType;

            await UploadAsync(blob, source, encryptKeyName);
        }
        public void WritePageBlob(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (source == null)
            {
                throw new ArgumentException("source");
            }

            using (MemoryStream stream = new MemoryStream(source))
            {
                WritePageBlob(containerName, filename, stream, contentType, encryptKeyName);
            }

        }

        public async Task WritePageBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (source == null)
            {
                throw new ArgumentException("source");
            }

            using (MemoryStream stream = new MemoryStream(source))
            {
               await WritePageBlobAsync(containerName, filename, stream, contentType, encryptKeyName);
            }
        }

        #endregion

        #region Append Blob Writers

        public void WriteAppendBlob(string containerName, string filename, Stream source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if(source == null)
            {
                throw new ArgumentNullException("source");
            }

            CloudBlobContainer container = GetContainerReference(containerName);
            CloudAppendBlob blob = container.GetAppendBlobReference(filename);

            if (!blob.Exists())
            {
                blob.Properties.ContentType = contentType;
                Upload(blob, source, encryptKeyName);
            }
            else
            {
                Append(blob, source, encryptKeyName);
            }
        }

        public async Task WriteAppendBlobAsync(string containerName, string filename, Stream source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudAppendBlob blob = container.GetAppendBlobReference(filename);

            if (!blob.Exists())
            {
                blob.Properties.ContentType = contentType;
                await UploadAsync(blob, source, encryptKeyName);
            }
            else
            {
                await AppendAsync(blob, source, encryptKeyName);
            }
        }
        public void WriteAppendBlob(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if(source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream stream = new MemoryStream(source))
            {
                WriteAppendBlob(containerName, filename, stream, contentType, encryptKeyName);
            }
        }
                
        public async Task WriteAppendBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", string encryptKeyName = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            using (MemoryStream stream = new MemoryStream(source))
            {
                await WriteAppendBlobAsync(containerName, filename, stream, contentType, encryptKeyName);
            }
        }

        
        #endregion

        #endregion

        #region Blob Readers

        #region Block Blob Readers

        public byte[] ReadBlockBlob(string containerName, string filename, string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            return Download(blob, encryptKeyName);          
        }

        public async Task<byte[]> ReadBlockBlobAsync(string containerName, string filename, string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            return await DownloadAsync(blob, encryptKeyName);
        }

        #endregion

        #region Page Blob Readers
        public byte[] ReadPageBlob(string containerName, string filename, string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = GetContainerReference(containerName);
            CloudPageBlob blob = container.GetPageBlobReference(filename);
            return Download(blob, encryptKeyName);
        }

        public async Task<byte[]> ReadPageBlobAsync(string containerName, string filename, string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudPageBlob blob = container.GetPageBlobReference(filename);
            return await DownloadAsync(blob, encryptKeyName);
        }
        #endregion

        #region Append Blob Readers
        public byte[] ReadAppendBlob(string containerName, string filename, string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = GetContainerReference(containerName);
            CloudAppendBlob blob = container.GetAppendBlobReference(filename);
            return Download(blob, encryptKeyName);
        }

        public async Task<byte[]> ReadAppendBlobAsync(string containerName, string filename, string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudAppendBlob blob = container.GetAppendBlobReference(filename);
            return await DownloadAsync(blob, encryptKeyName);
        }

        #endregion

        #endregion

        #region List Blobs

        public IEnumerable<IListBlobItem> ListBlobs(string containerName)
        {
            try
            {
                CloudBlobContainer container = GetContainerReference(containerName);
                return container.ListBlobs();
            }
            catch (Exception ex)
            {
                TraceManager.WriteWarning(101, containerName);
                TraceManager.WriteError(201, ex.Message);
                throw ex;
            }
        }

        public BlobResultSegment ListBlobsSegmented(string containerName, BlobContinuationToken token)
        {
            try
            {
                CloudBlobContainer container = GetContainerReference(containerName);
                return container.ListBlobsSegmented(token);
            }
            catch (Exception ex)
            {
                TraceManager.WriteWarning(101, containerName);
                TraceManager.WriteError(201, ex.Message);
                throw ex;
            }
        }

        public async Task<BlobResultSegment> ListBlobsSegmentedAsync(string containerName, BlobContinuationToken token)
        {
            try
            {
                CloudBlobContainer container = GetContainerReference(containerName);
                return await container.ListBlobsSegmentedAsync(token);
            }
            catch (Exception ex)
            {
                TraceManager.WriteWarning(101, containerName);
                TraceManager.WriteError(201, ex.Message);
                throw ex;
            }
        }


        #endregion

        #region Utilities

        public void Upload(ICloudBlob blob, Stream stream, string encryptKeyName = null)
        {
            if(encryptKeyName == null)
            {
                blob.UploadFromStream(stream);
            }
            else
            {
                BlobRequestOptions options = keyVault.GetEncryptionBlobOptions(encryptKeyName);
                blob.UploadFromStream(stream, null, options);
            }
        }

        private async Task UploadAsync(ICloudBlob blob, Stream stream, string encryptKeyName = null)
        {
            if (encryptKeyName == null)
            {
                await blob.UploadFromStreamAsync(stream);
            }
            else
            {
                BlobRequestOptions options = keyVault.GetEncryptionBlobOptions(encryptKeyName);
                await blob.UploadFromStreamAsync(stream, null, options, null);
            }
        }

        private void Append(CloudAppendBlob blob, Stream stream, string encryptKeyName = null)
        {
            if(string.IsNullOrEmpty(encryptKeyName))
            {
                blob.AppendBlock(stream);
            }
            else
            {
                BlobRequestOptions options = keyVault.GetEncryptionBlobOptions(encryptKeyName);
                blob.AppendBlock(stream, null, null, options);
            }
        }

        private async Task AppendAsync(CloudAppendBlob blob, Stream stream, string encryptKeyName = null)
        {
            if (string.IsNullOrEmpty(encryptKeyName))
            {
                await blob.AppendBlockAsync(stream);
            }
            else
            {
                BlobRequestOptions options = keyVault.GetEncryptionBlobOptions(encryptKeyName);
                await blob.AppendBlockAsync(stream, null, null, options, null);
            }
        }

        public byte[] Download(ICloudBlob blob, string encryptKeyName = null)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {
                if (string.IsNullOrEmpty(encryptKeyName))
                {
                    blob.DownloadToStream(stream);
                    buffer = stream.ToArray();
                }
                else
                {
                    BlobRequestOptions options = keyVault.GetEncryptionBlobOptions(encryptKeyName);
                    blob.DownloadToStream(stream, null, options);
                    buffer = stream.ToArray();
                }
            }

            return buffer;
        }

        public async Task<byte[]> DownloadAsync(ICloudBlob blob, string encryptKeyName = null)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {
                if (string.IsNullOrEmpty(encryptKeyName))
                {
                    await blob.DownloadToStreamAsync(stream);
                    buffer = stream.ToArray();
                }
                else
                {
                    BlobRequestOptions options = keyVault.GetEncryptionBlobOptions(encryptKeyName);
                    await blob.DownloadToStreamAsync(stream, null, options, null);
                    buffer = stream.ToArray();
                }
            }

            return buffer;
        }

        public CloudBlobContainer GetContainerReference(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                return client.GetContainerReference("$root");
            }
            else
            {
                CloudBlobContainer container = client.GetContainerReference(containerName);
                container.CreateIfNotExists();
                return container;
            }
        }

        public async Task<CloudBlobContainer> GetContainerReferenceAsync(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                return client.GetContainerReference("$root");
            }
            else
            {
                CloudBlobContainer container = client.GetContainerReference(containerName);
                await container.CreateIfNotExistsAsync();
                return container;
            }
        }
        #endregion 
    }
}
