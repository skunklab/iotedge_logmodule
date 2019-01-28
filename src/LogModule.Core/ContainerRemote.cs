using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogModule
{
    public class ContainerRemote : IContainerRemote
    {
        public event System.EventHandler<BytesTransferredEventArgs> OnDownloadBytesTransferred;
        public event System.EventHandler<BytesTransferredEventArgs> OnUploadBytesTransferred;
        public event System.EventHandler<BlobCompleteEventArgs> OnDownloadCompleted;
        public event System.EventHandler<BlobCompleteEventArgs> OnUploadCompleted;

        public static ContainerRemote Create(string accountName, string accountKey)
        {
            if(instance == null)
            {
                instance = new ContainerRemote(accountName, accountKey);
            }

            return instance;
        }

        protected ContainerRemote(string accountName, string accountKey)
        {
            operations = CommonIO.Create(accountName, accountKey);
            operations.OnDownloadBytesTransferred += Operations_OnDownloadBytesTransferred;
            operations.OnDownloadCompleted += Operations_OnDownloadCompleted;
            operations.OnUploadBytesTransferred += Operations_OnUploadBytesTransferred;
            operations.OnUploadCompleted += Operations_OnUploadCompleted;
        }

        protected CommonIO operations;
        private static ContainerRemote instance;

        public async Task DownloadFile(string path, string filename, string blobPath, string blobFilename, bool append = false, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (string.IsNullOrEmpty(blobPath))
            {
                throw new ArgumentNullException("blobPath");
            }

            if (string.IsNullOrEmpty(blobFilename))
            {
                throw new ArgumentNullException("blobFilename");
            }

            try
            {
                string fileToWrite = operations.FixPath(path) + filename;
                string container = operations.GetContainerName(blobPath);
                string fileToRead = operations.GetFilenameForContainer(blobPath, blobFilename);
                byte[] blob = await operations.ReadBlockBlobAsync(container, fileToRead, token);

                if (!token.IsCancellationRequested)
                {
                    if (append)
                    {
                        await operations.WriteAppendFileAsync(fileToWrite, blob);
                    }
                    else
                    {
                        await operations.WriteFileAsync(fileToWrite, blob);
                    }
                }
            }
            catch (Exception ex)
            {                
                Console.WriteLine(ex.StackTrace);
                operations.LogToDocker("DownloadFile", ex);                
                throw ex;
            }
        }

        public async Task DownloadFile(string path, string filename, string sasUri, bool append = false, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (string.IsNullOrEmpty(sasUri))
            {
                throw new ArgumentNullException("sasUri");
            }

            try
            {
                string fileToWrite = operations.FixPath(path) + filename;
                byte[] blob = await operations.ReadBlockBlobAsync(sasUri, token);

                if (!token.IsCancellationRequested)
                {
                    if (append)
                    {
                        await operations.WriteAppendFileAsync(fileToWrite, blob);
                    }
                    else
                    {
                        await operations.WriteFileAsync(fileToWrite, blob);
                    }
                }
            }
            catch (Exception ex)
            {
                operations.LogToDocker("DownloadFile", ex);
                throw ex;
            }
        }

        public async Task UploadFile(string path, string filename, string blobPath, string blobFilename, string contentType, bool append = false, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (string.IsNullOrEmpty(blobPath))
            {
                throw new ArgumentNullException("blobPath");
            }

            if (string.IsNullOrEmpty(blobFilename))
            {
                throw new ArgumentNullException("blobFilename");
            }

            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException("contentType");
            }

            try
            {
                string fileToRead = operations.FixPath(path) + filename;
                string container = operations.GetContainerName(blobPath);
                string fileToWrite = operations.GetFilenameForContainer(blobPath, blobFilename);

                if (!token.IsCancellationRequested)
                {
                    byte[] source = await operations.ReadFileAsync(fileToRead, token);
                    if (source != null)
                    {
                        if (append)
                        {
                            string crlf = "\r\n";
                            byte[] crlfBytes = Encoding.UTF8.GetBytes(crlf);
                            byte[] buffer = new byte[crlfBytes.Length + source.Length];
                            Buffer.BlockCopy(crlfBytes, 0, buffer, 0, crlfBytes.Length);
                            Buffer.BlockCopy(source, 0, buffer, crlfBytes.Length, source.Length);
                            await operations.WriteAppendBlobAsync(container, fileToWrite, buffer, contentType, token);
                        }
                        else
                        {
                            await operations.WriteBlockBlobAsync(container, fileToWrite, source, contentType, token);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                operations.LogToDocker("UploadFile", ex);
                throw ex;
            }
        }

        public async Task UploadFile(string path, string filename, string sasUri, string contentType, bool append = false, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (string.IsNullOrEmpty(sasUri))
            {
                throw new ArgumentNullException("sasUri");
            }

            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException("contentType");
            }

            try
            {
                string fileToRead = operations.FixPath(path) + filename;
                byte[] source = await operations.ReadFileAsync(fileToRead, token);
                if (source != null)
                {
                    if (append)
                    {
                        string crlf = "\r\n";
                        byte[] crlfBytes = Encoding.UTF8.GetBytes(crlf);
                        byte[] buffer = new byte[crlfBytes.Length + source.Length];
                        Buffer.BlockCopy(crlfBytes, 0, buffer, 0, crlfBytes.Length);
                        Buffer.BlockCopy(source, 0, buffer, crlfBytes.Length, source.Length);
                        await operations.WriteAppendBlobAsync(sasUri, buffer, contentType, token);
                    }
                    else
                    {
                        await operations.WriteBlockBlobAsync(sasUri, source, contentType, token);
                    }
                }
            }
            catch (Exception ex)
            {
                operations.LogToDocker("UploadFile", ex);
                throw ex;
            }
        }

        public async Task TruncateFile(string path, string filename, int maxBytes)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("sourcePath");
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("sourceFilename");
            }

            if (maxBytes < 0)
            {
                throw new ArgumentOutOfRangeException("maxBytes");
            }

            try
            {
                string srcFilename = operations.FixPath(path) + filename;
                byte[] content = await operations.ReadFileAsync(srcFilename, CancellationToken.None);

                if (content.Length > maxBytes)
                {
                    byte[] buffer = new byte[maxBytes];
                    Buffer.BlockCopy(content, content.Length - maxBytes, buffer, 0, maxBytes);
                    await operations.WriteFileAsync(srcFilename, buffer);
                }
            }
            catch (Exception ex)
            {
                operations.LogToDocker("TruncateFile", ex);
                throw ex;
            }
        }

        public async Task RemoveFile(string path, string filename)
        {
            try
            {
                string srcPath = operations.FixPath(path);
                string srcFilename = srcPath + filename;
                if (File.Exists(srcFilename))
                {
                    await operations.DeleteFileAsync(srcFilename);
                }
            }
            catch (Exception ex)
            {
                operations.LogToDocker("RemoveFile", ex);
                throw ex;
            }
        }

        public async Task CompressFile(string path, string filename, string compressPath, string compressFilename)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if(string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if(string.IsNullOrEmpty(compressPath))
            {
                throw new ArgumentNullException("compressPath");
            }

            if(string.IsNullOrEmpty(compressFilename))
            {
                throw new ArgumentNullException("compressFilename");
            }

            try
            {
                await operations.CompressFileAsync(path, filename, compressPath, compressFilename);
            }
            catch(Exception ex)
            {
                operations.LogToDocker("CompressFile", ex);
                throw ex;
            }
        }

        #region Events
        private void Operations_OnUploadCompleted(object sender, BlobCompleteEventArgs e)
        {
            OnUploadCompleted?.Invoke(this, e);
        }

        private void Operations_OnUploadBytesTransferred(object sender, BytesTransferredEventArgs e)
        {
            OnUploadBytesTransferred?.Invoke(this, e);
        }

        private void Operations_OnDownloadCompleted(object sender, BlobCompleteEventArgs e)
        {
            OnDownloadCompleted?.Invoke(this, e);
        }

        private void Operations_OnDownloadBytesTransferred(object sender, BytesTransferredEventArgs e)
        {
            OnDownloadBytesTransferred?.Invoke(this, e);
        }

        #endregion

    }
}
