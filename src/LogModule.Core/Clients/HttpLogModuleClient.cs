using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LogModule.Clients
{
    public class HttpLogModuleClient : IContainerLocal
    {
        public HttpLogModuleClient(string ipAddressOrHostname, int port)
        {
            
            ipAddress = ipAddressOrHostname.ToLowerInvariant() == "localhost" ? ipAddressOrHostname : GetIPAddress(ipAddressOrHostname);
            //ipAddress = GetIPAddress(ipAddressOrHostname);
            this.port = port;
        }

        private readonly string ipAddress;
        private readonly int port;

        public async Task DownloadFile(string path, string filename, string blobPath, string blobFilename, bool append = false, CancellationToken token = default(CancellationToken))
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/DownloadFile?path={2}&filename={3}&blobPath={4}&blobFilename={5}&append={6}", ipAddress, port, path, filename, blobPath, blobFilename, append);
                HttpResponseMessage response = await client.GetAsync(requestUri);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-DownloadFile : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task DownloadFile(string path, string filename, string sasUri, bool append = false, CancellationToken token = default(CancellationToken))
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/DownloadFile2?path={2}&filename={3}&sasUri={4}append={5}", ipAddress, port, path, filename, sasUri, append);
                HttpResponseMessage response = await client.GetAsync(requestUri);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-DownloadFile2 : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task<byte[]> GetFile(string path, string filename)
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/GetFile?path={2}&filename={3}", ipAddress, port, path, filename);               
                client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
                HttpResponseMessage response = await client.GetAsync(requestUri);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));                    
                }
                else
                {                    
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    return fileBytes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-GetFile : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task<string[]> ListFiles(string path)
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/ListFiles?path={2}", ipAddress, port, path);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                HttpResponseMessage response = await client.GetAsync(requestUri);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
                else
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<string[]>(jsonString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-ListFiles : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task RemoveFile(string path, string filename)
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/RemoveFile?path={2}&filename={3}", ipAddress, port, path, filename);
                HttpResponseMessage response = await client.DeleteAsync(requestUri);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-RemoveFile : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task TruncateFile(string path, string filename, int maxBytes)
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/TruncateFile?path={2}&filename={3}&maxBytes={4}", ipAddress, port, path, filename, maxBytes);
                HttpResponseMessage response = await client.PutAsync(requestUri, null);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-TruncateFile : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task UploadFile(string path, string filename, string blobPath, string blobFilename, string contentType, bool deleteOnUpload = false, TimeSpan? ttl = null, bool append = false, CancellationToken token = default(CancellationToken))
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = null;
                if (ttl.HasValue)
                {
                    requestUri = String.Format("http://{0}:{1}/api/Log/UploadFile?path={2}&filename={3}&blobPath={4}&blobFilename={5}&contentType={6}&append={7}&deleteonupload{8}&ttl{9}", ipAddress, port, path, filename, blobPath, blobFilename, contentType, append, deleteOnUpload, ttl.Value.ToString());
                }
                else
                {
                    requestUri = String.Format("http://{0}:{1}/api/Log/UploadFile?path={2}&filename={3}&blobPath={4}&blobFilename={5}&contentType={6}&append={7}&deleteonupload{8}", ipAddress, port, path, filename, blobPath, blobFilename, contentType, append, deleteOnUpload);
                }
               
                HttpResponseMessage response = await client.PostAsync(requestUri,null);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-UploadFile : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task UploadFile(string path, string filename, string sasUri, string contentType, bool deleteOnUpload = false, TimeSpan? ttl = null, bool append = false, CancellationToken token = default(CancellationToken))
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/UploadFile2?path={2}&filename={3}&sasUri={4}&contentType={5}&append={6}", ipAddress, port, path, filename, sasUri, contentType, append);

                HttpResponseMessage response = await client.PostAsync(requestUri, null);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-UploadFile2 : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task WriteFile(string path, string filename, byte[] body, bool append)
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/WriteFile?path={2}&filename={3}&append={4}", ipAddress, port, path, filename, append);
                
                HttpContent content = new ByteArrayContent(body);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                //client.DefaultRequestHeaders.Add("Content-Type", "application/octet-stream");

                HttpResponseMessage response = await client.PostAsync(requestUri, content);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-WriteFile : {0}", ex.Message);
                throw ex;
            }
        }

        public async Task CompressFile(string path, string filename, string compressPath, string compressFilename)
        {
            try
            {
                HttpClient client = new HttpClient();
                string requestUri = String.Format("http://{0}:{1}/api/Log/CompressFile?path={2}&filename={3}&compressPath={4}&compressFilename={5}", ipAddress, port, path, filename, compressPath, compressFilename);

                HttpResponseMessage response = await client.PostAsync(requestUri, null);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(String.Format("Status Code '{0}' with reason '{1}'", response.StatusCode, response.ReasonPhrase));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: HTTP-Client-CompressFile : {0}", ex.Message);
                throw ex;
            }
        }

        

        private string GetIPAddress(string ipAddressOrHostname)
        {
            IPAddress ipAddress = null;
            if(IPAddress.TryParse(ipAddressOrHostname, out ipAddress))
            {
                return ipAddress.ToString();
            }
           


            IPHostEntry entry = Dns.GetHostEntry(ipAddressOrHostname);
            string ipAddressString = null;

            foreach (var address in entry.AddressList)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (!address.ToString().Contains("127.0.0.1"))
                    {
                        ipAddressString = address.ToString();
                        break;
                    }

                }
            }

            return ipAddressString;
        }
    }
}
