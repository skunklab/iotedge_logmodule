using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap
{
    [Flags]
    public enum NoResponseType
    {
        All = 0,
        No200 = 2,
        No400 = 8,
        No500 = 16
    }
}
