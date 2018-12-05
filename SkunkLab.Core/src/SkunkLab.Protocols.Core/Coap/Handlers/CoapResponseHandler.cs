using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapResponseHandler : CoapMessageHandler
    {
        public CoapResponseHandler(CoapSession session, CoapMessage message)
            : base(session, message, null)
        {
            CoapAuthentication.EnsureAuthentication(session, message);
        }

        public override async Task<CoapMessage> ProcessAsync()
        {
            Session.CoapSender.DispatchResponse(Message);

            if(Message.MessageType == CoapMessageType.Acknowledgement)
            {
                return await Task.FromResult<CoapResponse>(new CoapResponse(Message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.EmptyMessage));
            }
            else
            {
                return null;
            }

        }
    }
}
