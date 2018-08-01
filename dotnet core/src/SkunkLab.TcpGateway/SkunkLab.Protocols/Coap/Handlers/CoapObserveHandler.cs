using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapObserveHandler : CoapMessageHandler
    {
        public CoapObserveHandler(CoapSession session, CoapMessage message, ICoapRequestDispatch dispatcher = null)
            : base(session, message, dispatcher)
        {   
                //Trace.WriteLine("Calling Observe Authentication");
                CoapAuthentication.EnsureAuthentication(session, message);
                //Trace.WriteLine("Observe Authentication called");
                //Trace.WriteLine(String.Format("Session authenticated is {0}", session.IsAuthenticated));
            
        }

        public override async Task<CoapMessage> ProcessAsync()
        {
            if (!Session.CoapReceiver.IsDup(Message.MessageId))
            {
                Session.CoapReceiver.CacheId(Message.MessageId);               
            }
         
            CoapMessage message = await Dispatcher.ObserveAsync(Message);

            return await Task.FromResult<CoapMessage>(message);
        }
    }
}
