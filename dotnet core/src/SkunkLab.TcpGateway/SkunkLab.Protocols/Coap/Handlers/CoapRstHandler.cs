using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapRstHandler : CoapMessageHandler
    {
        public CoapRstHandler(CoapSession session, CoapMessage message)
            : base(session, message, null)
        {

        }

        public override async Task<CoapMessage> ProcessAsync()
        {
            Session.CoapSender.Remove(Message.MessageId);
            return await Task.FromResult<CoapMessage>(Message);
        }
    }
}
