using LogModule.Models;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogModule.Hosts
{
    public class DirectMethodsHost
    {
       
        public DirectMethodsHost(ModuleClient client, ContainerRemote remote)
        {
            this.client = client;
            this.remote = remote;           
            uploadTokenSources = new Dictionary<string, CancellationTokenSource>();
            downloadTokenSources = new Dictionary<string, CancellationTokenSource>();
            this.remote.OnDownloadBytesTransferred += Remote_OnDownloadBytesTransferred;
            this.remote.OnDownloadCompleted += Remote_OnDownloadCompleted;
            this.remote.OnUploadBytesTransferred += Remote_OnUploadBytesTransferred;
            this.remote.OnUploadCompleted += Remote_OnUploadCompleted;
        }

        public event System.EventHandler<BytesTransferredEventArgs> OnDownloadBytesTransferred;
        public event System.EventHandler<BytesTransferredEventArgs> OnUploadBytesTransferred;
        public event System.EventHandler<BlobCompleteEventArgs> OnDownloadCompleted;
        public event System.EventHandler<BlobCompleteEventArgs> OnUploadCompleted;

       

        private ModuleClient client;
        private ContainerRemote remote;
        private Dictionary<string, CancellationTokenSource> uploadTokenSources;
        private Dictionary<string, CancellationTokenSource> downloadTokenSources;

        public void Init()
        {            
            client.SetMethodHandlerAsync("uploadFile", UploadFileHandler, null);
            client.SetMethodHandlerAsync("downloadFile", DownloadFileHandler, null);
            client.SetMethodHandlerAsync("removeFile", RemoveFileHandler, null);
            client.SetMethodHandlerAsync("truncateFile", TruncateFileHandler, null);
            client.SetMethodHandlerAsync("compressFile", CompressFileHandler, null);
            client.SetMethodHandlerAsync("listFiles", ListFilesHandler, null);
        }

        private async Task<MethodResponse> ListFilesHandler(MethodRequest request, object context)
        {
            string jsonString = request.DataAsJson;
            ListFilesModel model = JsonConvert.DeserializeObject<ListFilesModel>(jsonString);
            if(Directory.Exists(model.Path))
            {
                DirectoryInfo info = new DirectoryInfo(model.Path);
                FileInfo[] files = info.GetFiles();
                List<string> fileList = new List<string>();
                foreach(FileInfo file in files)
                {
                    fileList.Add(file.Name);
                }

                string jString = JsonConvert.SerializeObject(fileList.ToArray());
                return await Task.FromResult<MethodResponse>(new MethodResponse(Encoding.UTF8.GetBytes(jString), 200));
            }
            else
            {
                return await Task.FromResult<MethodResponse>(new MethodResponse(404));
            }

                
        }
        private async Task<MethodResponse> UploadFileHandler(MethodRequest request, object context)
        {
            int response = 200;

            try
            {
                string jsonString = request.DataAsJson;
                UploadFileModel model = JsonConvert.DeserializeObject<UploadFileModel>(jsonString);

                if (model.Cancel)
                {
                    string key = FixPath(model.BlobPath) + model.BlobFilename;

                    if (uploadTokenSources.ContainsKey(key))
                    {
                        CancellationTokenSource cts = uploadTokenSources[key];
                        cts.Cancel();
                    }
                    else
                    {
                        response = 404;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(model.SasUri))
                    {
                        string key = FixPath(model.BlobPath) + model.BlobFilename;

                        if (uploadTokenSources.ContainsKey(key))
                        {
                            uploadTokenSources.Remove(key);
                        }

                        await remote.UploadFile(model.Path, model.Filename, model.BlobPath, model.BlobFilename, model.ContentType, model.DeleteOnUpload, model.TTL, model.Append);                        
                    }
                    else
                    {
                        if (uploadTokenSources.ContainsKey(model.SasUri))
                        {
                            uploadTokenSources.Remove(model.SasUri);
                        }

                        uploadTokenSources.Add(model.SasUri, new CancellationTokenSource());
                        await remote.UploadFile(model.Path, model.Filename, model.SasUri, model.ContentType, model.DeleteOnUpload, model.TTL, model.Append);
                    }
                }
            }
            catch (Exception ex)
            {
                response = 500;
                Console.WriteLine("ERROR: DirectMethods-UploadFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }

        private async Task<MethodResponse> DownloadFileHandler(MethodRequest request, object context)
        {
            int response = 200;

            try
            {   
                string jsonString = request.DataAsJson;
                DownloadFileModel model = JsonConvert.DeserializeObject<DownloadFileModel>(jsonString);
                if (!model.Cancel)
                {
                    string key = FixPath(model.BlobPath) + model.BlobFilename;

                    if (downloadTokenSources.ContainsKey(key))
                    {
                        downloadTokenSources.Remove(key);
                    }

                    CancellationTokenSource cts = new CancellationTokenSource();
                    downloadTokenSources.Add(key, cts);

                    if (string.IsNullOrEmpty(model.SasUri))
                    {
                        //await remote.DownloadFile(model.Path, model.Filename, model.BlobPath, model.BlobFilename, model.Append);
                        InternalDownloadFile(model.Path, model.Filename, model.BlobPath, model.BlobFilename, model.Append, cts.Token);
                    }
                    else
                    {
                        InternalDownloadFile(model.Path, model.Filename, model.SasUri, model.Append, cts.Token);
                        //await remote.DownloadFile(model.Path, model.Filename, model.SasUri, model.Append, cts.Token);
                    }
                }
                else
                {
                    string key = FixPath(model.BlobPath) + model.BlobFilename;
                    if(downloadTokenSources.ContainsKey(key))
                    {
                        CancellationTokenSource cts = downloadTokenSources[key];
                        cts.Cancel();
                    }
                    downloadTokenSources.Remove(key);
                }
            }
            catch (Exception ex)
            {
                response = 500;
                Console.WriteLine("ERROR: DirectMethods-DownloadFileHandler '{0}'", ex.Message);
            }

            return await Task.FromResult<MethodResponse>(new MethodResponse(response));
        }

        private void InternalDownloadFile(string path, string filename, string blobPath, string blobFilename, bool append, CancellationToken token)
        {
            string key = FixPath(blobPath) + blobFilename;
            Task task = remote.DownloadFile(path, filename, blobPath, blobFilename, append, token).ContinueWith((a) => downloadTokenSources.Remove(key));
            Task.WhenAll(task);
        }

        private void InternalDownloadFile(string path, string filename, string sasUri, bool append, CancellationToken token)
        {
            string key = sasUri;
            Task task = remote.DownloadFile(path, filename, sasUri, append, token).ContinueWith((a) => downloadTokenSources.Remove(key));
            Task.WhenAll(task);
        }


        private async Task<MethodResponse> RemoveFileHandler(MethodRequest request, object context)
        {
            int response = 200;

            try
            {
                string jsonString = request.DataAsJson;
                RemoveFileModel model = JsonConvert.DeserializeObject<RemoveFileModel>(jsonString);
                await remote.RemoveFile(model.Path, model.Filename);
            }
            catch (Exception ex)
            {
                response = 500;
                Console.WriteLine("ERROR: DirectMethods-RemoveFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }

        private async Task<MethodResponse> TruncateFileHandler(MethodRequest request, object context)
        {
            int response = 200;

            try
            {
                string jsonString = request.DataAsJson;
                TruncateFileModel model = JsonConvert.DeserializeObject<TruncateFileModel>(jsonString);
                await remote.TruncateFile(model.Path, model.Filename, model.MaxBytes);
            }
            catch (Exception ex)
            {
                response = 500;
                Console.WriteLine("ERROR: DirectMethods-TruncateFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }

        private async Task<MethodResponse> CompressFileHandler(MethodRequest request, object context)
        {
            int response = 200;

            try
            {
                string jsonString = request.DataAsJson;
                CompressFileModel model = JsonConvert.DeserializeObject<CompressFileModel>(jsonString);
                await remote.CompressFile(model.Path, model.Filename, model.CompressPath, model.CompressFilename);
            }
            catch (Exception ex)
            {
                response = 500;
                Console.WriteLine("ERROR: DirectMethods-CompressFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }

        #region Events
        private void Remote_OnUploadCompleted(object sender, BlobCompleteEventArgs e)
        {
            if (uploadTokenSources.ContainsKey(e.Filename))
            {
                uploadTokenSources.Remove(e.Filename);
            }

            OnUploadCompleted?.Invoke(this, e);
        }

        private void Remote_OnUploadBytesTransferred(object sender, BytesTransferredEventArgs e)
        {
            OnUploadBytesTransferred?.Invoke(this, e);
        }

        private void Remote_OnDownloadCompleted(object sender, BlobCompleteEventArgs e)
        {
            if (downloadTokenSources.ContainsKey(e.Filename))
            {
                downloadTokenSources.Remove(e.Filename);
            }

            OnDownloadCompleted?.Invoke(this, e);
        }

        private void Remote_OnDownloadBytesTransferred(object sender, BytesTransferredEventArgs e)
        {
            OnDownloadBytesTransferred?.Invoke(this, e);
        }

        #endregion

        private string FixPath(string path)
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
    }
}
