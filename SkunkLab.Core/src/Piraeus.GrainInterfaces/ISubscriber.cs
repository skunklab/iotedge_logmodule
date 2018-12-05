using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Piraeus.GrainInterfaces
{
    public interface ISubscriber : IGrainWithStringKey
    {
        Task AddSubscriptionAsync(string subscriptionUriString);

        Task RemoveSubscriptionAsync(string subscriptionUriString);

        Task<IEnumerable<string>> GetSubscriptionsAsync();

        Task ClearAsync();
    }
}
