using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LogModule.Clients
{
    public class EdgeHubClient
    {
        public EdgeHubClient(ModuleClient client)
        {
            this.client = client;
            this.client.SetInputMessageHandlerAsync("getFileOutput", GetFileOutputHandler, this.client);
            this.client.SetInputMessageHandlerAsync("listFilesOutput", ListFilesOutputHandler, this.client);

        }

        public event EventHandler<GetFileEventArgs> OnGetFile;
        public event EventHandler<ListFileEventArgs> OnListFiles;

        private ModuleClient client;

        public async Task GetFile(string path, string filename)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            await client.SendEventAsync("getFile", message);
        }

        public async Task WriteFile(string path, string filename, byte[] body, bool append)
        {
            Message message = new Message(body);
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            message.Properties.Add("append", append.ToString());
            await client.SendEventAsync("writeFile", message);
        }

        public async Task ListFiles(string path)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            await client.SendEventAsync("listFiles", message);
        }

        public async Task UploadFile(string path, string filename, string blobPath, string blobFilename, string contentType, bool append = false)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            message.Properties.Add("blobPath", blobPath);
            message.Properties.Add("blobFilename", blobFilename);
            message.Properties.Add("contentType", contentType);
            message.Properties.Add("append", append.ToString());

            await client.SendEventAsync("uploadFile", message);
        }

        public async Task UploadFile(string path, string filename, string sasUri, string contentType, bool append = false)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            message.Properties.Add("sasUri", sasUri);
            message.Properties.Add("contentType", contentType);
            message.Properties.Add("append", append.ToString());

            await client.SendEventAsync("uploadFile2", message);
        }

        public async Task DownloadFile(string path, string filename, string blobPath, string blobFilename, bool append = false)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            message.Properties.Add("blobPath", blobPath);
            message.Properties.Add("blobFilename", blobFilename);
            message.Properties.Add("append", append.ToString());

            await client.SendEventAsync("downloadFile", message);
        }

        public async Task DownloadFile(string path, string filename, string sasUri, bool append = false)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            message.Properties.Add("sasUri", sasUri);
            message.Properties.Add("append", append.ToString());
            await client.SendEventAsync("downloadFile2", message);
        }

        public async Task TruncateFile(string path, string filename, int maxBytes)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            message.Properties.Add("maxBytes", maxBytes.ToString());

            await client.SendEventAsync("truncateFile", message);
        }

        public async Task RemoveFile(string path, string filename)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);

            await client.SendEventAsync("removeFile", message);
        }

        public async Task CompressFile(string path, string filename, string compressPath, string compressFilename)
        {
            Message message = new Message();
            message.Properties.Add("path", path);
            message.Properties.Add("filename", filename);
            message.Properties.Add("compressPath", compressPath);
            message.Properties.Add("compressFilename", compressFilename);

            await client.SendEventAsync("compressFile", message);
        }

        private Task<MessageResponse> GetFileOutputHandler(Message message, object context)
        {
            try
            {
                byte[] content = message.GetBytes();
                OnGetFile?.Invoke(this, new GetFileEventArgs(content));
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR Reading GetFileResponse - '{0}'", ex.Message);
            }

            return Task.FromResult<MessageResponse>(MessageResponse.Completed);
        }


        private Task<MessageResponse> ListFilesOutputHandler(Message message, object context)
        {
            try
            {
                byte[] content = message.GetBytes();
                if(content == null)
                {
                    OnListFiles?.Invoke(this, new ListFileEventArgs(null));
                }
                else
                {
                    string jsonString = Encoding.UTF8.GetString(content);
                    string[] list = JsonConvert.DeserializeObject<string[]>(jsonString);
                    OnListFiles?.Invoke(this, new ListFileEventArgs(list));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR Reading GetFileListResponse - '{0}'", ex.Message);
            }

            return Task.FromResult<MessageResponse>(MessageResponse.Completed);
        }
    }
}
