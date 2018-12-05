using LogModule.Models;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LogModule.DirectMethods
{
    public class DirectMethodsHost
    {
        public DirectMethodsHost(ModuleClient client, ContainerRemote remote)
        {
            this.client = client;
            this.remote = remote; 
        }

        private ModuleClient client;
        private ContainerRemote remote;

        public void Init()
        {
            
            client.SetMethodHandlerAsync("uploadFile", new MethodCallback(UploadFileHandler), client);
            client.SetMethodHandlerAsync("downloadFile", new MethodCallback(DownloadFileHandler), client);
            client.SetMethodHandlerAsync("removeFile", new MethodCallback(RemoveFileHandler), client);
            
            
        }

        private async Task<MethodResponse> UploadFileHandler(MethodRequest request, object context)
        {
            int response = 1;

            try
            {
                string jsonString = request.DataAsJson;
                UploadFileModel model = JsonConvert.DeserializeObject<UploadFileModel>(jsonString);

                if (string.IsNullOrEmpty(model.SasUri))
                {
                    await remote.UploadFile(model.Path, model.Filename, model.BlobPath, model.BlobFilename, model.ContentType, model.Append);
                }
                else
                {
                    await remote.UploadFile(model.Path, model.Filename, model.SasUri, model.ContentType, model.Append);
                }
            }
            catch(Exception ex)
            {
                response = -1;
                Console.WriteLine("ERROR: DirectMethods-UploadFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }

        private async Task<MethodResponse> DownloadFileHandler(MethodRequest request, object context)
        {
            int response = 1;

            try
            {
                string jsonString = request.DataAsJson;
                DownloadFileModel model = JsonConvert.DeserializeObject<DownloadFileModel>(jsonString);
                if (string.IsNullOrEmpty(model.SasUri))
                {
                    await remote.DownloadFile(model.Path, model.Filename, model.BlobPath, model.BlobFilename, model.Append);
                }
                else
                {
                    await remote.DownloadFile(model.Path, model.Filename, model.SasUri, model.Append);
                }
            }
            catch (Exception ex)
            {
                response = -1;
                Console.WriteLine("ERROR: DirectMethods-DownloadFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }

        private async Task<MethodResponse> RemoveFileHandler(MethodRequest request, object context)
        {
            int response = 1;

            try
            {
                string jsonString = request.DataAsJson;
                RemoveFileModel model = JsonConvert.DeserializeObject<RemoveFileModel>(jsonString);
                await remote.RemoveFile(model.Path, model.Filename);               
            }
            catch (Exception ex)
            {
                response = -1;
                Console.WriteLine("ERROR: DirectMethods-RemoveFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }

        private async Task<MethodResponse> TruncateFileHandler(MethodRequest request, object context)
        {
            int response = 1;

            try
            {
                string jsonString = request.DataAsJson;
                TruncateFileModel model = JsonConvert.DeserializeObject<TruncateFileModel>(jsonString);
                await remote.TruncateFile(model.Path, model.Filename, model.MaxBytes);
            }
            catch (Exception ex)
            {
                response = -1;
                Console.WriteLine("ERROR: DirectMethods-TruncateFileHandler '{0}'", ex.Message);
            }


            return new MethodResponse(response);
        }
    }
}
