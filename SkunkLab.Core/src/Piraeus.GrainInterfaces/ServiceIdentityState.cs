using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
    public class ServiceIdentityState
    {
        public byte[] Certificate { get; set; }

        public List<KeyValuePair<string,string>> Claims { get; set; }
    }
}
