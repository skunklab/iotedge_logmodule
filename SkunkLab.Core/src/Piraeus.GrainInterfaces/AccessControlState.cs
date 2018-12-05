using Capl.Authorization;
using System;

namespace Piraeus.GrainInterfaces
{
    [Serializable]
 
    public class AccessControlState
    {

        public AuthorizationPolicy Policy { get; set; }
    }
}
