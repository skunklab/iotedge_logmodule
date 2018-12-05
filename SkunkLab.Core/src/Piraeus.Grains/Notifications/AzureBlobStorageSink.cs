using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;


namespace Piraeus.Grains.Notifications
{
    public class AzureBlobStorageSink : EventSink
    {

        
        private string container;
        private string blobType;
        private string appendFilename;
        private Auditor auditor;
        private Uri uri;
        private string key;
        private Uri sasUri;        
        private BlobStorage[] storageArray;
        private int arrayIndex;
        private int clientCount;
        private string connectionString;
        private ConcurrentQueue<EventMessage> queue;



        public AzureBlobStorageSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            queue = new ConcurrentQueue<EventMessage>();          

            auditor = new Auditor();
            key = metadata.SymmetricKey;
            uri = new Uri(metadata.NotifyAddress);
            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            container = nvc["container"];
            
            if(!int.TryParse(nvc["clients"], out clientCount))
            {
                clientCount = 1;
            }

            if(!string.IsNullOrEmpty(nvc["file"]))
            {
                appendFilename = nvc["file"];
            }


            if (String.IsNullOrEmpty(container))
            {
                container = "$Root";
            }

            string btype = nvc["blobtype"];            

            if(String.IsNullOrEmpty(btype))
            {
                blobType = "block";
            }
            else
            {
                blobType = btype.ToLowerInvariant();
            }

            if (blobType != "block" &&
                blobType != "page" &&
                blobType != "append")
            {
                Trace.TraceWarning("Subscription {0} blob storage sink has invalid Blob Type of {1}", metadata.SubscriptionUriString, blobType);
                return;
            }

            sasUri = null;
            Uri.TryCreate(metadata.SymmetricKey, UriKind.Absolute, out sasUri);

            storageArray = new BlobStorage[clientCount];
            if (sasUri == null)
            {
                connectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", uri.Authority.Split(new char[] { '.' })[0], key);
                
                for(int i=0;i<clientCount;i++)
                {
                    storageArray[i] = BlobStorage.New(connectionString, 2048, 102400);
                }
            }
            else
            {
                connectionString = String.Format("BlobEndpoint={0};SharedAccessSignature={1}", container != "$Root" ? uri.ToString().Replace(uri.LocalPath, "") : uri.ToString(), key);
             
                for (int i = 0; i < clientCount; i++)
                {
                    storageArray[i] = BlobStorage.New(connectionString, 2048, 102400);
                }
            }
        }

        

        public override async Task SendAsync(EventMessage message)
        {            
            AuditRecord record = null;
            byte[] payload = null;
            EventMessage msg = null;
            queue.Enqueue(message);
            try
            {
                
                while (!queue.IsEmpty)
                {                    
                    bool isdequeued = queue.TryDequeue(out msg);
                    if (isdequeued)
                    {                         
                            arrayIndex = arrayIndex.RangeIncrement(0, clientCount - 1);

                            payload = GetPayload(msg);
                            if (payload == null)
                            {
                                Trace.TraceWarning("Subscription {0} could not write to blob storage sink because payload was either null or unknown protocol type.");
                                return;
                            }

                            string filename = GetBlobName(msg.ContentType);

                            if (blobType == "block")
                            {
                                Task task = storageArray[arrayIndex].WriteBlockBlobAsync(container, filename, payload, msg.ContentType);
                                Task innerTask = task.ContinueWith(async (a) => { await FaultTask(msg.MessageId, container, filename, payload, msg.ContentType, auditor.CanAudit && msg.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                                await Task.WhenAll(task);

                            }
                            else if (blobType == "page")
                            {
                                int pad = payload.Length % 512 != 0 ? 512 - payload.Length % 512 : 0;
                                byte[] buffer = new byte[payload.Length + pad];
                                Buffer.BlockCopy(payload, 0, buffer, 0, payload.Length);
                                Task task = storageArray[arrayIndex].WritePageBlobAsync(container, filename, buffer, msg.ContentType);
                                Task innerTask = task.ContinueWith(async (a) => { await FaultTask(msg.MessageId, container, filename, payload, msg.ContentType, auditor.CanAudit && msg.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                                await Task.WhenAll(task);
                            }
                            else
                            {
                                if (appendFilename == null)
                                {
                                    appendFilename = GetAppendFilename(msg.ContentType);
                                }

                                byte[] suffix = Encoding.UTF8.GetBytes(Environment.NewLine);
                                byte[] buffer = new byte[payload.Length + suffix.Length];
                                Buffer.BlockCopy(payload, 0, buffer, 0, payload.Length);
                                Buffer.BlockCopy(suffix, 0, buffer, payload.Length, suffix.Length);

                                Task task = storageArray[arrayIndex].WriteAppendBlobAsync(container, appendFilename, buffer, msg.ContentType);
                                Task innerTask = task.ContinueWith(async (a) => { await FaultTask(msg.MessageId, container, appendFilename, buffer, msg.ContentType, auditor.CanAudit && msg.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                                await Task.WhenAll(task);

                            }

                            record = new AuditRecord(msg.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
                    }                   
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Initial blob write error {0}", ex.Message);
                record = new AuditRecord(msg.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if (auditor.CanAudit && msg.Audit && record != null)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }


        }

        private async Task FaultTask(string id, string container, string filename, byte[] payload, string contentType, bool canAudit)
        {
            AuditRecord record = null;

            try
            {
                BlobStorage storage = BlobStorage.New(connectionString, 2048, 102400);

                if (blobType == "block")
                {
                    string[] parts = filename.Split(new char[] { '.' });
                    string path2 = parts.Length == 2 ? String.Format("{0}-R.{1}", parts[0], parts[1]) : String.Format("{0}-R", filename);

                    await storage.WriteBlockBlobAsync(container, path2, payload, contentType);
                }
                else if (blobType == "page")
                {
                    int pad = payload.Length % 512 != 0 ? 512 - payload.Length % 512 : 0;
                    byte[] buffer = new byte[payload.Length + pad];
                    Buffer.BlockCopy(payload, 0, buffer, 0, payload.Length);
                    await storage.WritePageBlobAsync(container, filename, buffer, contentType);
                }
                else
                {
                    await storage.WriteAppendBlobAsync(container, filename, payload, contentType);
                }

                record = new AuditRecord(id, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Retry Blob failed.");
                Trace.TraceError(ex.Message);
                record = new AuditRecord(id, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureBlob", "AzureBlob", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if(canAudit)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }


        }

        

        private byte[] GetPayload(EventMessage message)
        {
            switch(message.Protocol)
            {
                case ProtocolType.COAP:
                    CoapMessage coap = CoapMessage.DecodeMessage(message.Message);
                    return coap.Payload;
                case ProtocolType.MQTT:
                    MqttMessage mqtt = MqttMessage.DecodeMessage(message.Message);
                    return mqtt.Payload;
                case ProtocolType.REST:
                    return message.Message;
                case ProtocolType.WSN:
                    return message.Message;
                default:
                    return null;
            }
        }

        private string GetAppendFilename(string contentType)
        {
            if (appendFilename == null)
            {
                appendFilename = GetBlobName(contentType);
            }

            return appendFilename;
        }

        private string GetBlobName(string contentType)
        {
            string suffix = null;
            if (contentType.Contains("text"))
            {
                suffix = "txt";
            }
            else if (contentType.Contains("json"))
            {
                suffix = "json";
            }
            else if (contentType.Contains("xml"))
            {
                suffix = "xml";
            }

            string guid = Guid.NewGuid().ToString();
            string filename = String.Format("{0}T{1}", guid, DateTime.UtcNow.ToString("HH-MM-ss-fffff"));
            return suffix == null ? filename : String.Format("{0}.{1}", filename, suffix);
        }



    }
}
