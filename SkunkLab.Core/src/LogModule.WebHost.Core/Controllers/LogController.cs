using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace LogModule.WebHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        public LogController()
        {
            string accountName = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountName");
            string accountKey = System.Environment.GetEnvironmentVariable("LM_BlobStorageAccountKey");
            local = new ContainerLocal(accountName, accountKey);
        }

        private LogModule.ContainerLocal local;

        [HttpGet("DownloadFile")]
        public async Task<HttpResponseMessage> DownloadFile(string targetPath, string targetFilename, string containerName, string filename, bool append = false)
        {
            try
            {
                await local.DownloadFile(targetPath, targetFilename, containerName, filename, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }

        }

        [HttpGet("DownloadFile2")]
        public async Task<HttpResponseMessage> DownloadFile(string targetPath, string targetFilename, string sasUri, bool append = false)
        {
            try
            {
                await local.DownloadFile(targetPath, targetFilename, sasUri, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }
        }

        [HttpGet("GetFile")]
        public async Task<HttpResponseMessage> GetFile(string sourcePath, string sourceFilename)
        {
            try
            {
                byte[] content = await local.GetFile(sourcePath, sourceFilename);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(content) };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }
        }

        [HttpGet("ListFiles")]
        public async Task<HttpResponseMessage> ListFiles(string sourcePath)
        {
            try
            {
                string[] fileList = await local.ListFiles(sourcePath);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ObjectContent<string[]>(fileList, new JsonMediaTypeFormatter()) };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }

        }

        [HttpDelete("RemoveFile")]
        public async Task<HttpResponseMessage> RemoveFile(string sourcePath, string sourceFilename)
        {
            try
            {
                await RemoveFile(sourcePath, sourceFilename);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }
        }

        [HttpPut("TruncateFile")]
        public async Task<HttpResponseMessage> TruncateFile(string sourcePath, string sourceFilename, int maxBytes)
        {
            try
            {
                await TruncateFile(sourcePath, sourceFilename, maxBytes);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }

        }

        [HttpPost("UploadFile")]
        public async Task<HttpResponseMessage> UploadFile(string sourcePath, string sourceFilename, string containerName, string targetFilename, string contentType, bool append = false)
        {
            try
            {
                await local.UploadFile(sourcePath, sourceFilename, containerName, targetFilename, contentType, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }

        }

        [HttpPost("UploadFile2")]
        public async Task<HttpResponseMessage> UploadFile(string sourcePath, string sourceFilename, string sasUri, string contentType, bool append = false)
        {
            try
            {
                await local.UploadFile(sourcePath, sourceFilename, sasUri, contentType, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }
        }

        [HttpPut("WriteFile")]
        public async Task<HttpResponseMessage> WriteFile(string sourcePath, string sourceFilename, byte[] body, bool append)
        {
            try
            {
                await local.WriteFile(sourcePath, sourceFilename, body, append);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) { ReasonPhrase = ex.Message };
            }

        }
    }
}
