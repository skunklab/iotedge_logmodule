using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piraeus.GrainInterfaces
{
    public interface IServiceIdentity : IGrainWithStringKey
    {
        Task<byte[]> GetCertificateAsync();

        Task<List<KeyValuePair<string,string>>> GetClaimsAsync();

        Task AddCertificateAsync(byte[] certificate);

        Task AddClaimsAsync(List<KeyValuePair<string,string>> claims);
    }
}
