using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Core.Util;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class BlobStorage
    {
        private static BlobStorage instance;
        private CloudBlobClient client;
        private static SkunkLabBufferManager bufferManager;

        public event System.EventHandler<BytesTransferredEventArgs> OnUploadBytesTransferred;
        public event System.EventHandler<BlobCompleteEventArgs> OnUploadCompleted;
        public event System.EventHandler<BytesTransferredEventArgs> OnDownloadBytesTransferred;
        public event System.EventHandler<BlobCompleteEventArgs> OnDownloadCompleted;

        protected BlobStorage(string connectionString)
        {
            CloudStorageAccount account = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, Convert.ToBase64String(account.Credentials.ExportKey()));
            client = new CloudBlobClient(account.BlobStorageUri, credentials);
            client.DefaultRequestOptions.ParallelOperationThreadCount = 64;
            client.DefaultRequestOptions.SingleBlobUploadThresholdInBytes = 1048576;
            client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(10000), 8);
            client.DefaultRequestOptions.MaximumExecutionTime = TimeSpan.FromMinutes(5.0);

            if (bufferManager != null)
            {
                client.BufferManager = bufferManager;
            }
        }

        protected BlobStorage(string connectionString, string sasToken)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            StorageCredentials credentials = new StorageCredentials(sasToken);
            client = new CloudBlobClient(account.BlobStorageUri, credentials);
            client.DefaultRequestOptions.ParallelOperationThreadCount = 64;
            client.DefaultRequestOptions.SingleBlobUploadThresholdInBytes = 1048576;
            client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(10000), 8);
            client.DefaultRequestOptions.MaximumExecutionTime = TimeSpan.FromMinutes(3.0);
            

            if (bufferManager != null)
            {
                client.BufferManager = bufferManager;
            }
        }
        
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
        
        public static BlobStorage New(string connectionString, long maxBufferPoolSize = 0, int defaultBufferSize = 0)
        {
            if(maxBufferPoolSize > 0)
            {
                BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
                bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            }

            return new BlobStorage(connectionString);
        }
        
        public static BlobStorage New(string connectionString, string sasToken, long maxBufferPoolSize = 0, int defaultBufferSize = 0)
        {
            if (maxBufferPoolSize > 0)
            {
                BufferManager manager = BufferManager.CreateBufferManager(maxBufferPoolSize, defaultBufferSize);
                bufferManager = new SkunkLabBufferManager(manager, defaultBufferSize);
            }

            return new BlobStorage(connectionString, sasToken);
        }
        
        #region Blob Writers

        #region Block Blob Writers

        public async Task WriteBlockBlobAsync(string containerName, string filename, Stream source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, filename, bytesTransferred, source.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(filename);
                blob.Properties.ContentType = contentType;

                await blob.UploadFromStreamAsync(source, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch(Exception ex)
            {                
                if (ex.InnerException is TaskCanceledException)
                {
                    source = null;
                }
                else
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, filename, token.IsCancellationRequested, error));
            }
            
        }
       
        public async Task WriteBlockBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if(string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnDownloadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, filename, bytesTransferred, source.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(filename);
                blob.Properties.ContentType = contentType;

                await blob.UploadFromByteArrayAsync(source, 0, source.Length, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch(Exception ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    source = null;
                }
                else
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, filename, token.IsCancellationRequested, error));
            }
        }
        #endregion

        #region Page Blob Writers
                
        public async Task WritePageBlobAsync(string containerName, string filename, Stream source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, filename, bytesTransferred, source.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudPageBlob blob = container.GetPageBlobReference(filename);

                blob.Properties.ContentType = contentType;
                await blob.UploadFromStreamAsync(source, null, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch(Exception ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    source = null;
                }
                else
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, filename, token.IsCancellationRequested, error));
            }
        }

        public async Task WritePageBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (source == null)
            {
                throw new ArgumentException("source");
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, filename, bytesTransferred, source.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudPageBlob blob = container.GetPageBlobReference(filename);

                blob.Properties.ContentType = contentType;
                await blob.UploadFromByteArrayAsync(source, 0, source.Length, null, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    source = null;
                }
                else
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, filename, token.IsCancellationRequested, error));
            }
        }

        #endregion

        #region Append Blob Writers

      
        public async Task WriteAppendBlobAsync(string containerName, string filename, Stream source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, filename, bytesTransferred, source.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudAppendBlob blob = container.GetAppendBlobReference(filename);
                await blob.UploadFromStreamAsync(source, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch(Exception ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    source = null;
                }
                else
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, filename, token.IsCancellationRequested, error));
            }           
            
        }
                        
        public async Task WriteAppendBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, filename, bytesTransferred, source.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudAppendBlob blob = container.GetAppendBlobReference(filename);
                await blob.UploadFromByteArrayAsync(source, 0, source.Length, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is TaskCanceledException)
                {
                    source = null;
                }
                else
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, filename, token.IsCancellationRequested, error));
            }


        }




        #endregion
        
        #endregion

        #region Blob Readers

        #region Block Blob Readers

    
        public async Task<byte[]> ReadBlockBlobAsync(string containerName, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            return await DownloadAsync(blob);
        }

        #endregion

        #region Page Blob Readers
        

        public async Task<byte[]> ReadPageBlobAsync(string containerName, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudPageBlob blob = container.GetPageBlobReference(filename);
            return await DownloadAsync(blob);
        }
        #endregion

        #region Append Blob Readers       

        public async Task<byte[]> ReadAppendBlobAsync(string containerName, string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudAppendBlob blob = container.GetAppendBlobReference(filename);
            return await DownloadAsync(blob);
        }

        #endregion

        #endregion

        #region List Blobs

        public async Task<BlobResultSegment> ListBlobsAsync(string containerName, BlobContinuationToken token)
        {
            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            return await container.ListBlobsSegmentedAsync(token);
        }
        
     
        public async Task<BlobResultSegment> ListBlobsSegmentedAsync(string containerName, BlobContinuationToken token)
        {
            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                return await container.ListBlobsSegmentedAsync(token);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                throw ex;
            }
        }


        #endregion

        #region Direct File Upload/Download

        public async Task UploadFileToBlockBlob(string filePath, string containerName, string blobFilename, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filenamePath");
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException("containerName");
            }

            if (string.IsNullOrEmpty("blobFilename"))
            {
                throw new ArgumentNullException("blobFilename");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        FileInfo info = new FileInfo(filePath);
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, blobFilename, bytesTransferred, info.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(blobFilename);
                await blob.UploadFromFileAsync(filePath, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch (Exception ex)
            {
                if (!(ex.InnerException is TaskCanceledException))
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, blobFilename, token.IsCancellationRequested, error));
            }
        }

        public async Task DownloadBlockBlobToFile(string filePath, string containerName, string blobFilename, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filenamePath");
            }

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException("containerName");
            }

            if (string.IsNullOrEmpty("blobFilename"))
            {
                throw new ArgumentNullException("blobFilename");
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            Exception error = null;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;

            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        FileInfo info = new FileInfo(filePath);
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            OnDownloadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(containerName, blobFilename, bytesTransferred, info.Length));
                        }
                    });

            try
            {
                CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
                CloudBlockBlob blob = container.GetBlockBlobReference(blobFilename);
                await blob.DownloadToFileAsync(filePath, FileMode.Create, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
            }
            catch (Exception ex)
            {
                if (!(ex.InnerException is TaskCanceledException))
                {
                    error = ex;
                    throw ex;
                }
            }
            finally
            {
                watch.Stop();
                OnDownloadCompleted?.Invoke(this, new BlobCompleteEventArgs(containerName, blobFilename, token.IsCancellationRequested, error));
            }
        }

        #endregion

        #region Utilities



        private async Task UploadAsync(ICloudBlob blob, byte[] buffer)
        {
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                try
                {
                    await stream.FlushAsync();
                    stream.Close();
                }
                catch(Exception ex)
                {
                    Trace.TraceWarning("Blob upload failed with {0}", ex.Message);
                    throw ex;
                }
            }
            
        }

        private async Task UploadAsync(ICloudBlob blob, Stream stream)
        {
            try
            {
                await blob.UploadFromStreamAsync(stream);
            }
            catch (Exception ex)
            {                
                stream.Close();
                Trace.TraceWarning("Blob write failed.");
                Trace.TraceError(ex.Message);
                throw ex;
            }
        }
       
        private async Task AppendAsync(CloudAppendBlob blob, Stream stream)
        {
            try
            {
                await blob.AppendBlockAsync(stream);
                await stream.FlushAsync();
                stream.Close();
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Blob append failed.");
                Trace.TraceError(ex.Message);
                throw ex;
            }
        }
              
        public async Task<byte[]> DownloadAsync(ICloudBlob blob)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(stream);
                buffer = stream.ToArray();
            }

            return buffer;
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
