using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Piraeus.GrainInterfaces
{
    public interface IResourceList : IGrainWithStringKey
    {
        Task AddAsync(string resourceUriString);

        Task RemoveAsync(string resourceUriString);

        Task ClearAsync();

        Task<IEnumerable<string>> GetListAsync();

        Task<bool> Contains(string resourceUriString);
    }
}
