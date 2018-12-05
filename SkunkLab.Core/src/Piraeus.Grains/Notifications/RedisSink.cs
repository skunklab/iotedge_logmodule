using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using StackExchange.Redis;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Piraeus.Grains.Notifications
{
    public class RedisSink : EventSink
    {
        private string connectionString;
        private string cacheClaimType;
        private ConnectionMultiplexer connection;
        private IDatabase database;
        private int dbNumber;
        private TimeSpan? expiry;
        private Auditor auditor;
        private Uri uri;
        private TaskQueue tqueue;
        private ConcurrentQueueManager cqm;

        public RedisSink(SubscriptionMetadata metadata) 
            : base(metadata)
        {
            tqueue = new TaskQueue();
            cqm = new ConcurrentQueueManager();
            auditor = new Auditor();
            
            uri = new Uri(metadata.NotifyAddress);
            
            connectionString = String.Format("{0}:6380,password={1},ssl=True,abortConnect=False", uri.Authority, metadata.SymmetricKey);

            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);

            if(!int.TryParse(nvc["db"], out dbNumber))
            {
                dbNumber = -1;
            }

            TimeSpan expiration = TimeSpan.MaxValue;
            if(TimeSpan.TryParse(nvc["expiry"], out expiration))
            {
                expiry = expiration;
            }

            if(string.IsNullOrEmpty(metadata.ClaimKey))
            {
                cacheClaimType = metadata.ClaimKey;
            }

            Task<ConnectionMultiplexer> task = ConnectionMultiplexer.ConnectAsync(connectionString);
            Task.WaitAll(task);
            connection = task.Result;
        }

        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;
            byte[] payload = null;
            EventMessage msg = null;

            if (connection == null || !connection.IsConnected)
            {
                connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            }

            await tqueue.Enqueue(() => cqm.EnqueueAsync(message));

            try
            {
                while (!cqm.IsEmpty)
                {
                    msg = await cqm.DequeueAsync();
                    string cacheKey = GetKey(msg);

                    if (cacheKey == null)
                    {
                        Trace.TraceWarning("Redis sink has no cache key for subscription '{0}'.", this.metadata.SubscriptionUriString);
                        Trace.TraceError("No cache key found.");
                    }

                    payload = GetPayload(msg);

                    if (payload.Length == 0)
                    {
                        throw new InvalidOperationException("Payload length is 0.");
                    }


                    if (msg.ContentType != "application/octet-stream")
                    {
                        Task task = database.StringSetAsync(cacheKey, Encoding.UTF8.GetString(payload), expiry);
                        Task innerTask = task.ContinueWith(async (a) => { await FaultTask(msg, auditor.CanAudit && message.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                        await Task.WhenAll(task);
                    }
                    else
                    {
                        Task task = database.StringSetAsync(cacheKey, payload, expiry);
                        Task innerTask = task.ContinueWith(async (a) => { await FaultTask(msg, auditor.CanAudit && message.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                        await Task.WhenAll(task);
                    }

                    record = new AuditRecord(msg.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), String.Format("Redis({0})", dbNumber), String.Format("Redis({0})", dbNumber), payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);

                }

                //string cacheKey = GetKey(message);

                //if (cacheKey == null)
                //{
                //    Trace.TraceWarning("Redis sink has no cache key for subscription '{0}'.", this.metadata.SubscriptionUriString);
                //    Trace.TraceError("No cache key found.");
                //}

                

                //await ConnectAsync();
                //payload = GetPayload(message);

                //if(payload.Length == 0)
                //{
                //    throw new InvalidOperationException("Payload length is 0.");
                //}

                //if (message.ContentType != "application/octet-stream")
                //{
                //    Task task = database.StringSetAsync(cacheKey, Encoding.UTF8.GetString(payload), expiry);
                //    Task innerTask = task.ContinueWith(async (a) => { await FaultTask(message, auditor.CanAudit && message.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                //    await Task.WhenAll(task);
                //}
                //else
                //{
                //    Task task = database.StringSetAsync(cacheKey, payload, expiry);
                //    Task innerTask = task.ContinueWith(async (a) => { await FaultTask(message, auditor.CanAudit && message.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
                //    await Task.WhenAll(task);
                //}

                //record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), String.Format("Redis({0})", dbNumber), String.Format("Redis({0})", dbNumber), payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Initial Redis write error {0}", ex.Message);
                record = new AuditRecord(msg.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), String.Format("Redis({0})", dbNumber), String.Format("Redis({0})", dbNumber), payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if (auditor.CanAudit && message.Audit && record != null)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }
        }

        private async Task FaultTask(EventMessage message, bool canAudit)
        {
            AuditRecord record = null;
            IDatabase db = null;
            byte[] payload = null;
            ConnectionMultiplexer conn = null;

            try
            {
                string cacheKey = GetKey(message);

                conn = await NewConnection();
                
                if (dbNumber < 1)
                {
                    db = connection.GetDatabase();
                }
                else
                {
                    db = connection.GetDatabase(dbNumber);
                }

                payload = GetPayload(message);

                if (message.ContentType != "application/octet-stream")
                {
                    await db.StringSetAsync(cacheKey, Encoding.UTF8.GetString(payload), expiry);
                }
                else
                {
                    await db.StringSetAsync(cacheKey, payload, expiry);
                }

                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), String.Format("Redis({0})", db.Database), String.Format("Redis({0})", db.Database), payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
                record = new AuditRecord(message.MessageId, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), String.Format("Redis({0})", db.Database), String.Format("Redis({0})", db.Database), payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if (canAudit)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }

                if(conn != null)
                {
                    conn.Dispose();
                }
            }
        }


        public string GetKey(EventMessage message)
        {
            //if the cache key is sent on the message URI use it.  Otherwise, look for a matching claim type and return the value.
            if(!string.IsNullOrEmpty(message.CacheKey))
            {
                return message.CacheKey;
            }
            else if(!string.IsNullOrEmpty(cacheClaimType))
            {
                var principal = Thread.CurrentPrincipal as ClaimsPrincipal;
                var identity = new ClaimsIdentity(principal.Claims);
                if (identity.HasClaim((c) =>
                 {
                     return cacheClaimType.ToLowerInvariant() == c.Type.ToLowerInvariant();
                 }))
                {
                    Claim claim =
                    identity.FindFirst(
                        c =>
                            c.Type.ToLowerInvariant() ==
                            cacheClaimType.ToLowerInvariant());

                    return claim == null ? null : claim.Value;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private byte[] GetPayload(EventMessage message)
        {
            switch (message.Protocol)
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

        private async Task ConnectAsync()
        {
            if (connection == null || !connection.IsConnected)
            {
                connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
                if (dbNumber < 1)
                {
                    database = connection.GetDatabase();
                    dbNumber = database.Database;
                }
                else
                {
                    database = connection.GetDatabase(dbNumber);
                }               
            }
        }


        private async Task<ConnectionMultiplexer> NewConnection()
        {
            return await ConnectionMultiplexer.ConnectAsync(connectionString);
        }


    }
}
