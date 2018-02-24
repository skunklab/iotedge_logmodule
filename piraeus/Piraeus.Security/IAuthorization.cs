using System.Security.Claims;

namespace Piraeus.Security
{
    public interface IAuthorization
    {
        bool Authorize(ClaimsIdentity identity, string policyId);
    }
}
