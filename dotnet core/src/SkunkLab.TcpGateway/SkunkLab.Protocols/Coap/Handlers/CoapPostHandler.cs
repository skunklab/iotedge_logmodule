using System;
using System.Security;
using System.Threading.Tasks;
using SkunkLab.Protocols.Utilities;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapPostHandler : CoapMessageHandler
    {
        public CoapPostHandler(CoapSession session, CoapMessage message, ICoapRequestDispatch dispatcher = null) 
            : base(session, message, dispatcher)         
        {
            CoapAuthentication.EnsureAuthentication(session, message);
        }

        public override async Task<CoapMessage> ProcessAsync()
        {

            CoapMessage response = null;
            if (!Session.CoapReceiver.IsDup(Message.MessageId))
            {
                response = await Dispatcher.PostAsync(Message).ContinueWith<CoapMessage>(ExceptionAction, TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                if(Message.MessageType == CoapMessageType.Confirmable)
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

        private CoapMessage ExceptionAction(Task task)
        {
            System.Diagnostics.Trace.WriteLine(task.Exception.Message);
            System.Diagnostics.Trace.WriteLine(task.Exception.StackTrace);
            return null;
        }
    }
}
