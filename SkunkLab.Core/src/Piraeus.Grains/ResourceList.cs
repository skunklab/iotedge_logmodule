using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;
using Piraeus.GrainInterfaces;

namespace Piraeus.Grains
{
    [StorageProvider(ProviderName ="store")]
    public class ResourceList : Grain<ResourceListState>, IResourceList
    {
        public override Task OnActivateAsync()
        {
            if(State.Container == null)
            {
                State.Container = new List<string>();
            }

            return Task.CompletedTask;
        }

        public async override Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }

        public async Task AddAsync(string resourceUriString)
        {
            if(!State.Container.Contains(resourceUriString))
            {
                State.Container.Add(resourceUriString);
            }

            await Task.CompletedTask;
        }

        public async Task ClearAsync()
        {
            await ClearStateAsync();
        }

        public async Task<bool> Contains(string resourceUriString)
        {
            return await Task.FromResult<bool>(State.Container.Contains(resourceUriString));            
        }

        public async Task<IEnumerable<string>> GetListAsync()
        {
            return await Task.FromResult<IEnumerable<string>>(State.Container.ToArray());
        }

        public async Task RemoveAsync(string resourceUriString)
        {
            State.Container.Remove(resourceUriString);
            await Task.CompletedTask;
        }
    }
}
