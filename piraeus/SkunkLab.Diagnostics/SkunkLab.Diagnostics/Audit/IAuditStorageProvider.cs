using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Diagnostics.Audit
{
    public interface IAuditStorageProvider
    {
        Task WriteRecordAsync(AuditRecord record);

    }
}
