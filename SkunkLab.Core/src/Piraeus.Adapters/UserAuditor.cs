using Piraeus.Core;
using SkunkLab.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class UserAuditor
    {
        public UserAuditor()
            : this(System.Environment.GetEnvironmentVariable("ORLEANS_AUDIT_DATACONNECTIONSTRING"), System.Environment.GetEnvironmentVariable("USER_AUDIT_TABLENAME"))
        {
        }

        public UserAuditor(string connectionstring, string tablename = "userlog")
        {
            this.connectionstring = connectionstring;

            if (!string.IsNullOrEmpty(this.connectionstring))
            {
                storage = TableStorage.New(this.connectionstring, 2048, 102400);
            }

            this.tablename = tablename;
        }

        private TableStorage storage;
        private string tablename;
        private string connectionstring;

        public bool CanAudit
        {
            get { return (storage != null && tablename != null); }
        }

        public async Task<UserLogRecord> GetAuditRecordAsync(string channelId, string identity)
        {
            List<UserLogRecord> list = await storage.ReadAsync<UserLogRecord>(this.tablename, channelId, identity);

            if (list != null && list.Count == 1)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        public UserLogRecord GetAuditRecord(string channelId, string identity)
        {
            Task<List<UserLogRecord>> task = storage.ReadAsync<UserLogRecord>(this.tablename, channelId, identity);
            Task.WaitAll(task);
            List<UserLogRecord> list = task.Result;

            if (list != null && list.Count == 1)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        public async Task WriteAuditRecordAsync(UserLogRecord record)
        {
            if (storage != null && record != null)
            {
                try
                {
                    await storage.WriteAsync(tablename, record);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("Auditor failed to write record.");
                    Trace.TraceError("Auditor error {0}", ex.Message);
                    Trace.TraceError("Auditor stacktrace {0}", ex.StackTrace);
                }
            }
        }

        public void WriteAuditRecord(UserLogRecord record)
        {
            if (storage != null && record != null)
            {
                storage.WriteAsync(tablename, record).Ignore();
            }
        }
    }
}
