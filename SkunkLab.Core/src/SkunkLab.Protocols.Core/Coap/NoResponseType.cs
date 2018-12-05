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

    public static class NoResponseExtensions
    {
        public static bool IsNoResponse(this NoResponseType? nrt, CodeType code)
        {
            if(!nrt.HasValue)
            {
                return false;
            }

            int value = (int)code;

            if (nrt.Value.HasFlag(NoResponseType.All))
            {
                return true;
            }
            else if(value >= 200 && value < 300)
            {
                return nrt.Value.HasFlag(NoResponseType.No200);
            }
            else if(value >= 400 && value < 500)
            {
                return nrt.Value.HasFlag(NoResponseType.No400);
            }
            else if(value >= 500)
            {
                return nrt.Value.HasFlag(NoResponseType.No500);
            }
            else
            {
                return false;
            }
            
            

        }
    }
}
