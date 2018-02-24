using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Security
{
    public interface IAuthenticator
    {
        bool Authenticate(SecurityTokenType type, byte[] token);

        bool Authenticate(SecurityTokenType type, string token);
        
    }
}
