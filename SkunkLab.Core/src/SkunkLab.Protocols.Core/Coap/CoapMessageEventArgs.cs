using System;

namespace SkunkLab.Protocols.Coap
{
    public class CoapMessageEventArgs : EventArgs
    {
        public CoapMessageEventArgs(CoapMessage message)
        {
            Message = message;
        }

        public CoapMessage Message { get; set; }
    }
}
