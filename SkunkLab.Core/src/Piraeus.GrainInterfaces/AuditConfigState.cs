using System;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class AuditConfigState
    {
        public string ConnectionString { get; set; }

        public string Tablename { get; set; }
    }
}
