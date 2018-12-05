using Orleans;
using Orleans.Providers;
using Piraeus.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Grains
{
    [StorageProvider(ProviderName = "store")]
    public class AuditConfig : Grain<AuditConfigState>, IAuditConfig
    {

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }

        public Task SetAzureStorageConnectionAsync(string connectionstring, string tablename)
        {
            State.ConnectionString = connectionstring;
            State.Tablename = tablename;
            return WriteStateAsync();
        }

        public Task<string> GetConnectionstringAsync()
        {
            return Task.FromResult<string>(State.ConnectionString);
        }

        public Task<string> GetTableNameAsync()
        {
            return Task.FromResult<string>(State.Tablename);
        }
    }
}
