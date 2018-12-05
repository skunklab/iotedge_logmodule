using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapGetHandler : CoapMessageHandler
    {
        public CoapGetHandler(CoapSession session, CoapMessage message, ICoapRequestDispatch dispatcher = null)
            : base(session, message, dispatcher)
        {
            CoapAuthentication.EnsureAuthentication(session, message);
        }

        public override async Task<CoapMessage> ProcessAsync()
        {
            CoapMessage response = null;
            if (!Session.CoapReceiver.IsDup(Message.MessageId))
            {
                response = await Dispatcher.GetAsync(Message);
            }
            else
            {
                if (Message.MessageType == CoapMessageType.Confirmable)
                {
                    return await Task.FromResult<CoapMessage>(new CoapResponse(Message.MessageId, ResponseMessageType.Acknowledgement, ResponseCodeType.EmptyMessage));
                }
            }

            if (response != null && !Message.NoResponse.IsNoResponse(Message.Code))
            {
                return response;
            }
            else
            {
                return null;
            }

        }
    }
}
