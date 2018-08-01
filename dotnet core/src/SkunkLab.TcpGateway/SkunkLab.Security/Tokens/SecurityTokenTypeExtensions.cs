using System;

namespace SkunkLab.Security.Tokens
{
    public static class SecurityTokenTypeExtensions
    {
        public static SecurityTokenType ToSecurityTokenType(this SecurityTokenType type, string tokenType)
        {
            return (SecurityTokenType)Enum.Parse(typeof(SecurityTokenType), tokenType, true);
        }
    }
}
