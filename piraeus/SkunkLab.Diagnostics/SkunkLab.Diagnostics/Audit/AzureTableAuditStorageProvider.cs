//using System.Threading.Tasks;
//using SkunkLab.Storage;

//namespace SkunkLab.Diagnostics.Audit
//{
//    public class AzureTableAuditStorageProvider : IAuditStorageProvider
//    {
//        public AzureTableAuditStorageProvider(string tableName, string connectionString, long maxBufferPoolSize, int defaultBufferSize)
//        {
//            table = tableName;
//            storage = TableStorage.New(connectionString, maxBufferPoolSize, defaultBufferSize);
//        }        

//        private TableStorage storage;
//        private string table;
        

//        public async Task WriteRecordAsync(AuditRecord record)
//        {
//            await storage.WriteAsync(table, record);
//        }
//    }
//}
