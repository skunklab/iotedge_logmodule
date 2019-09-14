using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Core.Util;
using System.Diagnostics;

namespace LogModule
{
    public class CommonIO
    {
        public event System.EventHandler<BytesTransferredEventArgs> OnDownloadBytesTransferred;
        public event System.EventHandler<BytesTransferredEventArgs> OnUploadBytesTransferred;
        public event System.EventHandler<BlobCompleteEventArgs> OnDownloadCompleted;
        public event System.EventHandler<BlobCompleteEventArgs> OnUploadCompleted;

        protected CommonIO(string accountName, string accountKey)
        {
            if (string.IsNullOrEmpty(accountName))
            {
                throw new ArgumentException("accountName");
            }

            if (string.IsNullOrEmpty(accountKey))
            {
                throw new ArgumentException("accountKey");
            }

            string connectionString = "DefaultEndpointsProtocol=https;" + "AccountName=" + accountName + ";AccountKey=" + accountKey + ";EndpointSuffix=core.windows.net";
            writeContainer = new Dictionary<string, ConcurrentQueue<byte[]>>();
            readContainer = new HashSet<string>();

            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            StorageCredentials credentials = new StorageCredentials(account.Credentials.AccountName, Convert.ToBase64String(account.Credentials.ExportKey()));
            client = new CloudBlobClient(account.BlobStorageUri, credentials);           
            client.DefaultRequestOptions.ParallelOperationThreadCount = 64;
            client.DefaultRequestOptions.SingleBlobUploadThresholdInBytes = 1048576;
            client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(10000), 8);
            client.DefaultRequestOptions.MaximumExecutionTime = TimeSpan.FromMinutes(5.0);
        }

        public static CommonIO Create(string accountName, string accountKey)
        {
            if (instance == null)
            {
                instance = new CommonIO(accountName, accountKey);
            }

            return instance;
        }

        public static CommonIO instance;

        public string accountName;
        public string accountKey;
        private CloudBlobClient client;
        private Dictionary<string, ConcurrentQueue<byte[]>> writeContainer;
        private HashSet<string> readContainer;

        #region Local File Operations
                
        public async Task<byte[]> ReadFileAsync(string filename, CancellationToken token)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            await AccessWaitAsync(filename, TimeSpan.FromSeconds(5));

            readContainer.Add(filename.ToLowerInvariant());
            byte[] message = null;

            try
            {

                using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    
                    int bytesRead = 0;
                    do
                    {
                        if (token.IsCancellationRequested)
                        {
                            message = null;
                            break;
                        }

                        byte[] buffer = new byte[ushort.MaxValue];
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if(message == null)
                        {
                            message = new byte[bytesRead];
                            Buffer.BlockCopy(buffer, 0, message, 0, bytesRead);
                        }
                        else
                        {
                            byte[] temp = new byte[message.Length + buffer.Length];
                            Buffer.BlockCopy(message, 0, temp, 0, message.Length);
                            Buffer.BlockCopy(buffer, 0, temp, message.Length, buffer.Length);
                            message = temp;
                        }
                    } while (bytesRead > 0);

                    stream.Close();
                }

                return message;
            }
            finally
            {
                readContainer.Remove(filename.ToLowerInvariant());
                if (token.IsCancellationRequested)
                {
                    OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(filename, true));
                }
            }

        }

        
        public async Task WriteFileAsync(string filename, byte[] data)
        {
            ConcurrentQueue<byte[]> queue = null;

            await AccessWaitAsync(filename.ToLowerInvariant(), TimeSpan.FromSeconds(5));

            if (writeContainer.ContainsKey(filename.ToLowerInvariant()))
            {
                queue = writeContainer[filename.ToLowerInvariant()];
            }
            else
            {
                queue = new ConcurrentQueue<byte[]>();
                writeContainer.Add(filename.ToLowerInvariant(), queue);
            }

            queue.Enqueue(data);
            try
            {
                while (queue.Count > 0)
                {
                    byte[] buffer = null;

                    if (queue.TryDequeue(out buffer))
                    {
                        using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
                        {
                            await stream.WriteAsync(buffer, 0, buffer.Length);
                            await stream.FlushAsync();
                            stream.Close();
                        }
                    }
                }
            }
            finally
            {
                writeContainer.Remove(filename.ToLowerInvariant());
            }
        }
        public async Task WriteAppendFileAsync(string filename, byte[] data)
        {
            ConcurrentQueue<byte[]> queue = null;

            await AccessWaitAsync(filename.ToLowerInvariant(), TimeSpan.FromSeconds(5));

            if (writeContainer.ContainsKey(filename.ToLowerInvariant()))
            {
                queue = writeContainer[filename.ToLowerInvariant()];
            }
            else
            {
                queue = new ConcurrentQueue<byte[]>();
                writeContainer.Add(filename.ToLowerInvariant(), queue);
            }

            queue.Enqueue(data);

            try
            {
                bool exists = File.Exists(filename);
                using (FileStream stream = new FileStream(filename, FileMode.Append))
                {
                    while (queue.Count > 0)
                    {
                        byte[] block = null;
                        byte[] buffer = null;
                        if (queue.TryDequeue(out block))
                        {
                            if (exists)
                            {
                                string cr = "\r\n";
                                byte[] prepend = Encoding.UTF8.GetBytes(cr);
                                buffer = new byte[prepend.Length + block.Length];
                                Buffer.BlockCopy(prepend, 0, buffer, 0, prepend.Length);
                                Buffer.BlockCopy(block, 0, buffer, prepend.Length, block.Length);
                            }
                            else
                            {
                                exists = true;
                                buffer = block;
                                Console.WriteLine("<<< {0} >>>", Encoding.UTF8.GetString(buffer));
                            }

                            await stream.WriteAsync(buffer, 0, buffer.Length);
                            await stream.FlushAsync();
                        }
                    }
                }
            }
            finally
            {
                //writeContainer.Remove(filename.ToLowerInvariant());
            }
        }
        public async Task DeleteFileAsync(string filename)
        {
            if(!File.Exists(filename))
            {
                throw new FileNotFoundException("File not found.");
            }

            await AccessWaitAsync(filename.ToLowerInvariant(), TimeSpan.FromSeconds(5));
            if (!readContainer.Contains(filename.ToLowerInvariant()))
            {
                readContainer.Add(filename.ToLowerInvariant());
            }

            try
            {  
                File.Delete(filename);
            }
            finally
            {
                readContainer.Remove(filename.ToLowerInvariant());
            }

        }
        public async Task CompressFileAsync(string path, string filename, string compressPath, string compressFilename)
        {
            string zFilename = FixPath(compressPath) + compressFilename;
            string srcFilename = FixPath(path) + filename;

            if (!File.Exists(srcFilename))
            {
                return;
            }

            await AccessWaitAsync(srcFilename, TimeSpan.FromSeconds(5));

            readContainer.Add(srcFilename.ToLowerInvariant());

            try
            {
                if(File.Exists(zFilename))
                {
                    File.Delete(zFilename);
                }

                using (ZipArchive archive = ZipFile.Open(zFilename, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(srcFilename, filename);
                }
            }
            finally
            {
                readContainer.Remove(filename.ToLowerInvariant());
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Blob Operations
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

        public async Task<byte[]> ReadBlockBlobAsync(string containerName, string filename, CancellationToken token)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            

            return await DownloadAsync(blob, token);
        }

        public async Task<byte[]> ReadBlockBlobAsync(string sasUri, CancellationToken token)
        {
            if (string.IsNullOrEmpty(sasUri))
            {
                throw new ArgumentException("sasUri");
            }

            CloudBlockBlob blob = new CloudBlockBlob(new Uri(sasUri));
            return await DownloadAsync(blob, token);
        }

        public async Task WriteBlockBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = contentType;
           
            await UploadAsync(blob, source, token);
        }

        //public async Task WriteBlockBlobAsync(string containerName, string filename, 
        //                    byte[] source, string contentType, CancellationToken token = default(CancellationToken))
        //{
        //    if (source == null)
        //    {
        //        throw new ArgumentNullException("source");
        //    }

        //    CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
        //    CloudBlockBlob blob = container.GetBlockBlobReference(filename);
        //    blob.Properties.ContentType = contentType;

        //    await UploadAsync(blob, source, token);
        //}

        public async Task WriteAppendBlobAsync(string containerName, string filename, byte[] source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            CloudBlobContainer container = await GetContainerReferenceAsync(containerName);
            CloudAppendBlob blob = container.GetAppendBlobReference(filename);

            if (!await blob.ExistsAsync())
            {
                blob.Properties.ContentType = contentType;
                await UploadAsync(blob, source, token);
            }
            else
            {
                await blob.AppendFromByteArrayAsync(source, 0, source.Length);
            }
        }

        public async Task WriteBlockBlobAsync(string sasUri, byte[] source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            CloudBlockBlob blob = new CloudBlockBlob(new Uri(sasUri));
            blob.Properties.ContentType = contentType;
            await UploadAsync(blob, source, token);
        }

        public async Task WriteAppendBlobAsync(string sasUri, byte[] source, string contentType = "application/octet-stream", CancellationToken token = default(CancellationToken))
        {
            CloudAppendBlob blob = new CloudAppendBlob(new Uri(sasUri));
            blob.Properties.ContentType = contentType;
            await UploadAsync(blob, source, token);
        }

        private async Task<CloudBlobContainer> GetContainerReferenceAsync(string containerName)
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
        private async Task<byte[]> DownloadAsync(ICloudBlob blob)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {                
                await blob.DownloadToStreamAsync(stream);
                buffer = stream.ToArray();
            }

            return buffer;
        }

        private async Task<byte[]> DownloadAsync(CloudBlockBlob blob, CancellationToken token)
        {
            string fullpath = blob.Container.Name + "/" + blob.Name;
            byte[] buffer = null;
            Stopwatch watch = new Stopwatch();
            using (MemoryStream stream = new MemoryStream())
            {                
                watch.Start();
                double time = watch.Elapsed.TotalMilliseconds;
                long bytesTransferred = 0;
                IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                    progress =>
                    {
                        bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                        if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                        {
                            time = time + 1000.0;
                            OnDownloadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(fullpath, blob.Properties.Length, progress.BytesTransferred));
                        }
                    });

                try
                {
                    await blob.DownloadToStreamAsync(stream, default(AccessCondition),
                                                            default(BlobRequestOptions), default(OperationContext),
                                                            progressHandler, token);

                    buffer = stream.ToArray();
                    watch.Stop();
                }
                catch(Exception ex)
                {
                    watch.Stop();
                    if (ex.InnerException is TaskCanceledException)
                    {
                        buffer = null;
                    }
                    else
                    {
                        throw ex;
                    }
                }

                OnDownloadCompleted?.Invoke(this, new BlobCompleteEventArgs(fullpath, token.IsCancellationRequested));
            }

            return buffer;
        }

        private async Task UploadAsync(CloudAppendBlob blob, byte[] buffer, CancellationToken token)
        {
            string fullpath = blob.Container.Name + "/" + blob.Name;

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
                            OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(fullpath, buffer.Length, progress.BytesTransferred));
                        }
                    });

            try
            {
                await blob.UploadFromByteArrayAsync(buffer, 0, buffer.Length, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), progressHandler, token);
                watch.Stop();
            }
            catch (Exception ex)
            {
                watch.Stop();
                if (ex.InnerException is TaskCanceledException)
                {
                    buffer = null;
                }
                else
                {
                    throw ex;
                }
            }

            OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(fullpath, token.IsCancellationRequested));
        }

        private async Task UploadAsync(CloudBlockBlob blob, byte[] buffer, CancellationToken token)
        {
            string fullpath = blob.Container.Name + "/" + blob.Name;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            double time = watch.Elapsed.TotalMilliseconds;
            long bytesTransferred = 0;
            BlobRequestOptions options = new BlobRequestOptions();
            options.SingleBlobUploadThresholdInBytes = 1048576;
            
            
            IProgress<StorageProgress> progressHandler = new Progress<StorageProgress>(
                   progress =>
                   {
                       bytesTransferred = bytesTransferred < progress.BytesTransferred ? progress.BytesTransferred : bytesTransferred;
                       if (watch.Elapsed.TotalMilliseconds > time + 1000.0 && bytesTransferred <= progress.BytesTransferred)
                       {
                           OnUploadBytesTransferred?.Invoke(this, new BytesTransferredEventArgs(fullpath, buffer.Length, progress.BytesTransferred));
                       }
                   });

            try
            {  
                await blob.UploadFromByteArrayAsync(buffer, 0, buffer.Length, default(AccessCondition), options, default(OperationContext), progressHandler, token);
            }
            catch (Exception ex)
            {
                watch.Stop();
                if (ex.InnerException is TaskCanceledException)
                {
                    buffer = null;
                }
                else
                {
                    throw ex;
                }
            }
            OnUploadCompleted?.Invoke(this, new BlobCompleteEventArgs(fullpath, token.IsCancellationRequested));
        }

        #endregion

        #region Interanl file IO checks
        private bool CanAccess(string filename)
        {
            return !readContainer.Contains(filename.ToLowerInvariant()) && (!writeContainer.ContainsKey(filename.ToLowerInvariant()) || (writeContainer.ContainsKey(filename.ToLowerInvariant()) && writeContainer[filename.ToLowerInvariant()].Count == 0));
        }
        private async Task AccessWaitAsync(string filename, TimeSpan maxWaitTime)
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (!CanAccess(filename))
            {
                await Task.Delay(100);
                if (stopwatch.ElapsedMilliseconds > maxWaitTime.TotalMilliseconds)
                {
                    break;
                }
            }

            stopwatch.Stop();
        }

        #endregion

        #region Helpers
        public string FixPath(string path)
        {
            if (path.Contains("/") && !(path[path.Length - 1] == '/'))
            {
                return path + "/";
            }
            else if (path.Contains("\\") && !(path[path.Length - 1] == '\\'))
            {
                return path + "\\";
            }
            else
            {
                return path;
            }
        }

        public string GetFilename(string path, string filename, int maxSize = 0)
        {
            string fqn = FixPath(path) + filename;

            if (maxSize > 0 && File.Exists(fqn))
            {
                FileInfo info = new FileInfo(fqn);
                if (info.Length > Convert.ToInt64(maxSize))
                {
                    RenameFile(path, filename);
                }
            }

            return fqn;
        }
        

        public void RenameFile(string path, string filename)
        {
            string fqn = FixPath(path) + filename;
            FileInfo srcInfo = new FileInfo(fqn);
            string ext = srcInfo.Extension;

            long ticks = DateTime.UtcNow.Ticks;
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            long unix = dto.ToUnixTimeSeconds();
            string epoch = unix.ToString();

            string srcShortName = srcInfo.Name.Replace(srcInfo.Extension, "");
            string newFile = FixPath(path) + String.Format($"{srcShortName.ToLowerInvariant()}_{epoch}") + ext;
            File.Move(fqn, newFile);
            File.Delete(fqn);

        }

        public string GetContainerName(string path)
        {
            string container = null;

            if (!path.Contains('/'))
            {
                container = path;
            }
            else
            {
                string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                container = parts[0];
            }
            return container;
        }

        public string GetFilenameForContainer(string path, string filename)
        {
            string filepath = null;

            if (!path.Contains('/'))
            {
                filepath = filename;
            }
            else
            {
                string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                StringBuilder builder = new StringBuilder();
                if (parts.Length > 1)
                {
                    int index = 1;
                    while (index < parts.Length)
                    {
                        builder.Append(parts[index] + "/");
                        index++;
                    }
                    builder.Append(filename);
                    filepath = builder.ToString();
                }
                else
                {
                    filepath = filename;
                }
            }

            return filepath;
        }

        public void LogToDocker(string id, Exception ex)
        {
            Console.WriteLine("{0} error {1}", id, ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine("{0} inner error {1}", id, ex.InnerException.Message);
            }
        }
        #endregion
    }
}
