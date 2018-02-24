using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapSession
    {
        public CoapSession(CoapConfig config)
        {
            this.config = config;
        }

        private CoapConfig config;

        public void Publish(string resourceUriString, string contentType, byte[] message)
        {
            //Convert to CoAP
            //Set message ID
            //Response option (CON or NONCON)
        }

    }
}
