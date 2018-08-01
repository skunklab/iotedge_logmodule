using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public abstract class CoapMessageHandler
    {
        public static CoapMessageHandler Create(CoapSession session, CoapMessage message, ICoapRequestDispatch dispatcher = null)
        {      
            if(message.Code == CodeType.EmptyMessage && message.MessageType == CoapMessageType.Confirmable)
            {
                return new CoapPingHandler(session, message);
            }
            else if(message.Code == CodeType.POST) 
            {
                return new CoapPostHandler(session, message, dispatcher);
            }
            else if(message.Code == CodeType.PUT)
            {
                return new CoapPutHandler(session, message, dispatcher);
            }
            else if(message.Code == CodeType.GET)
            {
                return new CoapObserveHandler(session, message, dispatcher);
            }
            else if(message.Code == CodeType.DELETE)
            {
                return new CoapDeleteHandler(session, message, dispatcher);
            }
            else if(message.MessageType == CoapMessageType.Reset)
            {
                return new CoapRstHandler(session, message);
            }            
            else
            {
                return new CoapResponseHandler(session, message);
            }
        }

        protected CoapMessageHandler(CoapSession session, CoapMessage message, ICoapRequestDispatch dispatcher = null)
        {
            Session = session;
            Message = message;
            Dispatcher = dispatcher;            
        }

        protected ICoapRequestDispatch Dispatcher { get; set; }

        protected CoapSession Session { get; set; }

        protected CoapMessage Message { get; set; }

        public abstract Task<CoapMessage> ProcessAsync();

       
    }
}
