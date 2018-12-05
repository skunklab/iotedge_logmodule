using System;

namespace Piraeus.Core.Metadata
{
    [Serializable]
    public enum SecurityTokenType
    {
        None,
        Jwt,
        X509
    }
}
