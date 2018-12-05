using System;

namespace SkunkLab.Protocols.Coap
{
    [Flags]
    public enum CoapConfigOptions
    {
        None = 0,
        Observe = 1,
        NoResponse = 2
    }
}
