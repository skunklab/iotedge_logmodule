using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogModule
{
    public interface IContainerRemote
    {
        //Task UploadFile(string path, string filename, string blobPath, string blobFilename, string contentType, bool append = false, CancellationToken token = default(CancellationToken));
        Task UploadFile(string path, string filename, string blobPath, string blobFilename, string contentType, bool deleteOnUpload = false, TimeSpan? ttl = null,  bool append = false, CancellationToken token = default(CancellationToken));
        //Task UploadFile(string path, string filename, string sasUri, string contentType, bool append = false, CancellationToken token = default(CancellationToken));

        Task UploadFile(string path, string filename, string sasUri, string contentType, bool deleteOnUpload = false, TimeSpan? ttl = null, bool append = false, CancellationToken token = default(CancellationToken));

        Task DownloadFile(string path, string filename, string blobPath, string blobFilename, bool append = false, CancellationToken token = default(CancellationToken));
        Task DownloadFile(string path, string filename, string sasUri, bool append = false, CancellationToken token = default(CancellationToken));
        Task TruncateFile(string path, string filename, int maxBytes);
        Task RemoveFile(string path, string filename);


    }
}
