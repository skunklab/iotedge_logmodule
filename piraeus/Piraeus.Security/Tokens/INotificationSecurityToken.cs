

namespace Piraeus.Security.Tokens
{
    using System.Net;
    public interface INotificationSecurityToken
    {
        void SetSecurityToken(HttpWebRequest request);
    }
}
