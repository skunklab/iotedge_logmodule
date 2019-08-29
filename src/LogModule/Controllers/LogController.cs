using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace LogModule.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        public LogController()
        {            
            string accountName = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountName");
            string accountKey = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountKey");
            local = ContainerLocal.Create(accountName, accountKey);
        }

        private LogModule.ContainerLocal local;

        [HttpGet("DownloadFile")]
        public async Task<HttpResponseMessage> DownloadFile(string path, string filename, string blobPath, string blobFilename, bool append = false)
        {
            try
            {
                await local.DownloadFile(path, filename, blobPath, blobFilename, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DownloadFile - {ex.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); // { ReasonPhrase = ex.Message };
            }

        }

        [HttpGet("DownloadFile2")]
        public async Task<HttpResponseMessage> DownloadFile(string path, string filename, string sasUri, bool append = false)
        {
            try
            {
                await local.DownloadFile(path, filename, sasUri, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DownloadFile2 - {ex.Message}");
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }
        }

        [HttpGet("GetFile")]
        public async Task<HttpResponseMessage> GetFile(string path, string filename)
        {
            try
            {
                byte[] content = await local.GetFile(path, filename);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(content) };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }
        }

        [HttpGet("ListFiles")]
        public async Task<HttpResponseMessage> ListFiles(string path)
        {
            try
            {
                string[] fileList = await local.ListFiles(path);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ObjectContent<string[]>(fileList, new JsonMediaTypeFormatter()) };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }

        }

        [HttpDelete("RemoveFile")]
        public async Task<HttpResponseMessage> RemoveFile(string path, string filename)
        {
            try
            {                
                await local.RemoveFile(path, filename);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }
        }

        [HttpPut("TruncateFile")]
        public async Task<HttpResponseMessage> TruncateFile(string path, string filename, int maxBytes)
        {
            try
            {
                await local.TruncateFile(path, filename, maxBytes);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);  //{ ReasonPhrase = ex.Message };
            }

        }

        [HttpPost("UploadFile")]
        public async Task<HttpResponseMessage> UploadFile(string path, string filename, string blobPath, string blobFilename, string contentType, bool append = false, bool deleteOnUpload = false, TimeSpan? ttl = null)
        {
            try
            {
                await local.UploadFile(path, filename, blobPath, blobFilename, contentType, deleteOnUpload, ttl, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }

        }

        [HttpPost("UploadFile2")]
        public async Task<HttpResponseMessage> UploadFile(string path, string filename, string sasUri, string contentType, bool append = false, bool deleteOnUpload = false, TimeSpan? ttl = null)
        {
            try
            {
                await local.UploadFile(path, filename, sasUri, contentType, deleteOnUpload, ttl,  append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }
        }

        [HttpPost("WriteFile")]
        public async Task<HttpResponseMessage> WriteFile([FromBody] byte[] body, string path, string filename, bool append, int maxSize = 0)
        {
            try
            {
                await local.WriteFile(path, filename, body, append, maxSize);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }

        }

        [HttpPost("CompressFile")]
        public async Task<HttpResponseMessage> CompressFile(string path, string filename, string compressPath, string compressFilename)
        {
            try
            {
                await local.CompressFile(path, filename, compressPath, compressFilename);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError); //{ ReasonPhrase = ex.Message };
            }
        }
    }
}