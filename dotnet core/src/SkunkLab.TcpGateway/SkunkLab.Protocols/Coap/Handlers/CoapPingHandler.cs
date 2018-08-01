using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapPingHandler : CoapMessageHandler
    {
        public CoapPingHandler(CoapSession session, CoapMessage message)
            :base(session, message, null)
        {
            CoapAuthentication.EnsureAuthentication(session, message);
        }
        public override async Task<CoapMessage> ProcessAsync()
        {            
            return await Task.FromResult<CoapMessage>(new CoapResponse(Message.MessageId, ResponseMessageType.Reset, ResponseCodeType.EmptyMessage));
        }
    }
}
