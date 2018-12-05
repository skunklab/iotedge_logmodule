using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LogModule.EdgeHub
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
        }

        public async Task<MessageResponse> GetFileHandler(Message message, object context)
        {
            try
            {
                string sourcePath = message.Properties["sourcePath"];
                string sourceFilename = message.Properties["sourceFilename"];
                byte[] content = await local.GetFile(sourcePath, sourceFilename);

                //return the output
                Message output = new Message(content);
                output.Properties.Add("sourcePath", sourcePath);
                output.Properties.Add("sourceFilename", sourceFilename);
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
                string targetPath = message.Properties["targetPath"];
                string targetFilename = message.Properties["targetFilename"];
                bool append = message.Properties.ContainsKey("append") ? Convert.ToBoolean(message.Properties["append"]) : false;
                string containerName = message.Properties.ContainsKey("containerName") ? message.Properties["containerName"] : null;
                string filename = message.Properties.ContainsKey("filename") ? message.Properties["filename"] : null;
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
                string sourcePath = message.Properties["sourcePath"];
                string sourceFilename = message.Properties["sourceFilename"];
                string contentType = message.Properties["contentType"];
                string containerName = message.Properties.ContainsKey("containerName") ? message.Properties["containerName"] : null;
                string targetFilename = message.Properties.ContainsKey("targetFilename") ? message.Properties["targetFilename"] : null;
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
                string sourcePath = message.Properties["sourcePath"];
                string sourceFilename = message.Properties["sourceFilename"];
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
                string sourcePath = message.Properties["sourcePath"];
                string sourceFilename = message.Properties["sourceFilename"];
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
                string sourcePath = message.Properties["sourcePath"];
                string[] files = await local.ListFiles(sourcePath);

                //return the output
                string jsonString = JsonConvert.SerializeObject(files);
                Message output = new Message(Encoding.UTF8.GetBytes(jsonString));
                output.Properties.Add("sourcePath", sourcePath);
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

                string sourcePath = message.Properties["sourcePath"];
                string sourceFilename = message.Properties["sourceFilename"];
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

    }
}
