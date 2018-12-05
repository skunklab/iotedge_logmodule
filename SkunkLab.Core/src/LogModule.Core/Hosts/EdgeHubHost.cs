using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogModule.Hosts
{
    public class EdgeHubLogHost
    {
        public EdgeHubLogHost(ModuleClient client, LogModule.ContainerLocal local)
        {
            this.client = client;
            this.local = local;
        }

        private ModuleClient client;
        private LogModule.ContainerLocal local;

        public void Init()
        {
            client.SetInputMessageHandlerAsync("getFile", GetFileHandler, client);
            client.SetInputMessageHandlerAsync("downloadFile", DownloadFileHandler, client);
            client.SetInputMessageHandlerAsync("uploadFile", UploadFileHandler, client);
            client.SetInputMessageHandlerAsync("writeFile", WriteFileHandler, client);
            client.SetInputMessageHandlerAsync("removeFile", RemoveFileHandler, client);
            client.SetInputMessageHandlerAsync("listFiles", ListFilesHandler, client);
            client.SetInputMessageHandlerAsync("truncateFile", TruncateFileHandler, client);
            client.SetInputMessageHandlerAsync("compressFile", CompressFileHandler, client);
        }

        public async Task<MessageResponse> GetFileHandler(Message message, object context)
        {
            try
            {
                string sourcePath = message.Properties["path"];
                string sourceFilename = message.Properties["filename"];
                byte[] content = await local.GetFile(sourcePath, sourceFilename);

                //return the output
                Message output = new Message(content);
                output.Properties.Add("path", sourcePath);
                output.Properties.Add("filename", sourceFilename);
                await client.SendEventAsync("getFileOutput", output);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-GetFileHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;
        }

        public async Task<MessageResponse> DownloadFileHandler(Message message, object context)
        {
            try
            {
                string targetPath = message.Properties["path"];
                string targetFilename = message.Properties["filename"];
                bool append = message.Properties.ContainsKey("append") ? Convert.ToBoolean(message.Properties["append"]) : false;
                string containerName = message.Properties.ContainsKey("blobPath") ? message.Properties["blobPath"] : null;
                string filename = message.Properties.ContainsKey("blobFilename") ? message.Properties["blobFilename"] : null;
                string sasUri = message.Properties.ContainsKey("sasUri") ? message.Properties["sasUri"] : null;


                if (sasUri != null)
                {
                    await local.DownloadFile(targetPath, targetFilename, sasUri, append);
                }
                else
                {
                    await local.DownloadFile(targetPath, targetFilename, containerName, filename, append);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-DownloadFileHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;
        }

        public async Task<MessageResponse> UploadFileHandler(Message message, object context)
        {
            try
            {
                string sourcePath = message.Properties["path"];
                string sourceFilename = message.Properties["filename"];
                string contentType = message.Properties["contentType"];
                string containerName = message.Properties.ContainsKey("blobPath") ? message.Properties["blobPath"] : null;
                string targetFilename = message.Properties.ContainsKey("blobFilename") ? message.Properties["blobFilename"] : null;
                string sasUri = message.Properties.ContainsKey("sasUri") ? message.Properties["sasUri"] : null;
                bool append = message.Properties.ContainsKey("append") ? Convert.ToBoolean(message.Properties["append"]) : false;

                if (sasUri == null)
                {
                    await local.UploadFile(sourcePath, sourceFilename, containerName, targetFilename, contentType, append);
                }
                else
                {
                    await local.UploadFile(sourcePath, sourceFilename, sasUri, contentType, append);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-EdgeHub-UploadFileHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;

        }

        public async Task<MessageResponse> WriteFileHandler(Message message, object context)
        {
            try
            {
                string sourcePath = message.Properties["path"];
                string sourceFilename = message.Properties["filename"];
                bool append = message.Properties.ContainsKey("append") ? Convert.ToBoolean(message.Properties["append"]) : false;
                byte[] content = message.GetBytes();
                await local.WriteFile(sourcePath, sourceFilename, content, append);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-WriteFileHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;

        }

        public async Task<MessageResponse> RemoveFileHandler(Message message, object context)
        {
            try
            {
                string sourcePath = message.Properties["path"];
                string sourceFilename = message.Properties["filename"];
                await local.RemoveFile(sourcePath, sourceFilename);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-RemoveFileHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;

        }

        public async Task<MessageResponse> ListFilesHandler(Message message, object context)
        {
            try
            {
                string sourcePath = message.Properties["path"];
                string[] files = await local.ListFiles(sourcePath);

                //return the output
                string jsonString = JsonConvert.SerializeObject(files);
                Message output = new Message(Encoding.UTF8.GetBytes(jsonString));
                output.Properties.Add("path", sourcePath);
                await client.SendEventAsync("listFilesOutput", output);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-ListFilesHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;
        }

        public async Task<MessageResponse> TruncateFileHandler(Message message, object context)
        {
            try
            {

                string sourcePath = message.Properties["path"];
                string sourceFilename = message.Properties["filename"];
                int maxBytes = Convert.ToInt32(message.Properties["maxBytes"]);

                await local.TruncateFile(sourcePath, sourceFilename, maxBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-TruncateFileHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;

        }

        public async Task<MessageResponse> CompressFileHandler(Message message, object context)
        {
            try
            {

                string sourcePath = message.Properties["path"];
                string sourceFilename = message.Properties["filename"];
                string compressPath = message.Properties["compressPath"];
                string compressFilename = message.Properties["compressFilename"];

                await local.CompressFile(sourcePath, sourceFilename, compressPath, compressFilename);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: EdgeHub-CompressFileHandler '{0}'", ex.Message);
            }
            finally
            {
                ModuleClient mc = (ModuleClient)context;
                await mc.CompleteAsync(message);
            }

            return MessageResponse.Completed;
        }

    }
}
