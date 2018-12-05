

using SkunkLab.Storage;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Piraeus.Grains.Notifications
{
    public class Auditor
    {
        public Auditor()
            : this(System.Environment.GetEnvironmentVariable("ORLEANS_AUDIT_DATACONNECTIONSTRING"), System.Environment.GetEnvironmentVariable("ORLEANS_AUDIT_TABLENAME"))
        {   
        }


        public Auditor(string connectionstring, string tablename)
        {
            this.connectionstring = connectionstring;

            if(!string.IsNullOrEmpty(this.connectionstring))
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

        public async Task WriteAuditRecordAsync(AuditRecord record)
        {
            if (storage != null && record != null)
            {
                try
                {
                    await storage.WriteAsync(tablename, record);
                }
                catch(Exception ex)
                {
                    Trace.TraceWarning("Auditor failed to write record.");
                    Trace.TraceError("Auditor error {0}", ex.Message);
                    Trace.TraceError("Auditor stacktrace {0}", ex.StackTrace);
                }
            }
        }

        public void WriteAuditRecord(AuditRecord record)
        {
            if (storage != null && record != null)
            {
                try
                {
                    Task task = storage.WriteAsync(tablename, record);
                    Task.WhenAll(task);
                }
                catch(Exception ex)
                {
                    Trace.TraceWarning("Auditor failed to write record.");
                    Trace.TraceError("Auditor error {0}", ex.Message);
                    Trace.TraceError("Auditor stacktrace {0}", ex.StackTrace);
                }
            }
        }
    }


}
