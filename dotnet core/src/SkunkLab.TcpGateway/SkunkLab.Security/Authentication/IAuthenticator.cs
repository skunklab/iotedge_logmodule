using SkunkLab.Security.Tokens;

namespace SkunkLab.Security.Authentication
{
    public interface IAuthenticator
    {
        bool Authenticate(SecurityTokenType type, byte[] token);

        bool Authenticate(SecurityTokenType type, string token);

    }
}
