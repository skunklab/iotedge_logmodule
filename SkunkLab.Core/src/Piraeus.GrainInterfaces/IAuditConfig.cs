using System.Threading.Tasks;
using Orleans;

namespace Piraeus.GrainInterfaces
{
    public interface IAuditConfig : IGrainWithStringKey
    {
        
        Task SetAzureStorageConnectionAsync(string connectionstring, string tablename);

        Task<string> GetConnectionstringAsync();

        Task<string> GetTableNameAsync();
    }
}
