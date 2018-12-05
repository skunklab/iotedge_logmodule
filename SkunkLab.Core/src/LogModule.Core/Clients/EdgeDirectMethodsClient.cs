using LogModule.Models;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogModule.Clients
{
    public class EdgeDirectMethodsClient : IContainerRemote
    {
        public EdgeDirectMethodsClient(string deviceId, string moduleId, ModuleClient client)
        {
            this.deviceId = deviceId;
            this.moduleId = moduleId;
            this.client = client;
            this.client.SetRetryPolicy(new ExponentialBackoff(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(10)));
        }

        private ModuleClient client;
        private readonly string deviceId;
        private readonly string moduleId;

        public async Task UploadFile(string path, string filename, string blobPath, string blobFilename, string contentType, bool append = false, CancellationToken token = default(CancellationToken))
        {
            UploadFileModel model = new UploadFileModel(path, filename, blobPath, blobFilename, contentType, append);
            byte[] message = GetMessage(model);
            string jstring = JsonConvert.SerializeObject(model);
            MethodRequest request = new MethodRequest("uploadFile", Encoding.UTF8.GetBytes(jstring));            
            MethodResponse response = await client.InvokeMethodAsync(deviceId, moduleId, request);
            if(response.Status != 200)
            {
                Console.WriteLine("DirectMethods Client UploadFile failed");
            }
        }
        
        public async Task UploadFile(string path, string filename, string sasUri, string contentType, bool append = false, CancellationToken token = default(CancellationToken))
        {
            UploadFileModel model = new UploadFileModel(path, filename, sasUri, contentType, append);
            byte[] message = GetMessage(model);
            MethodRequest request = new MethodRequest("uploadFile", message);
            MethodResponse response = await client.InvokeMethodAsync(deviceId, moduleId, request);
            if (response.Status != 200)
            {
                Console.WriteLine("DirectMethods Client UploadFile2 failed");
            }

        }

        public async Task DownloadFile(string path, string filename, string blobPath, string blobFilename, bool append = false, CancellationToken token = default(CancellationToken))
        {
            DownloadFileModel model = new DownloadFileModel(path, filename, blobPath, blobFilename, append);            
            byte[] message = GetMessage(model);
            MethodRequest request = new MethodRequest("downloadFile", message);
            MethodResponse response = await client.InvokeMethodAsync(deviceId, moduleId, request);
            if (response.Status != 200)
            {
                Console.WriteLine("DirectMethods Client DownloadFile failed");
            }

        }

        public async Task DownloadFile(string path, string filename, string sasUri, bool append = false, CancellationToken token = default(CancellationToken))
        {
            DownloadFileModel model = new DownloadFileModel(path, filename, sasUri, append);
            byte[] message = GetMessage(model);
            MethodRequest request = new MethodRequest("downloadFile", message);
            MethodResponse response = await client.InvokeMethodAsync(deviceId, moduleId, request);
            if (response.Status != 200)
            {
                Console.WriteLine("DirectMethods Client DownloadFile2 failed");
            }
        }

        public async Task TruncateFile(string path, string filename, int maxBytes)
        {
            TruncateFileModel model = new TruncateFileModel(path, filename, maxBytes);
            byte[] message = GetMessage(model);
            MethodRequest request = new MethodRequest("truncateFile", message);
            MethodResponse response = await client.InvokeMethodAsync(deviceId, moduleId, request);

            if (response.Status != 200)
            {
                Console.WriteLine("DirectMethods Client TruncateFile failed");
            }

        }

        public async Task RemoveFile(string path, string filename)
        {
            RemoveFileModel model = new RemoveFileModel(path, filename);
            byte[] message = GetMessage(model);
            MethodRequest request = new MethodRequest("removeFile", message);
            MethodResponse response = await client.InvokeMethodAsync(deviceId, moduleId, request);

            if (response.Status != 200)
            {
                Console.WriteLine("DirectMethods Client RemoveFile failed");
            }
        }

        public async Task CompressFile(string path, string filename, string compressPath, string compressFilename)
        {
            CompressFileModel model = new CompressFileModel(path, filename, compressPath, compressFilename);
            byte[] message = GetMessage(model);
            MethodRequest request = new MethodRequest("compressFile", message);
            MethodResponse response = await client.InvokeMethodAsync(deviceId, moduleId, request);

            if (response.Status != 200)
            {
                Console.WriteLine("DirectMethods Client CompressFile failed");
            }

        }

        private byte[] GetMessage(object jsonObject)
        {
            string jsonString = JsonConvert.SerializeObject(jsonObject);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        
    }
}
