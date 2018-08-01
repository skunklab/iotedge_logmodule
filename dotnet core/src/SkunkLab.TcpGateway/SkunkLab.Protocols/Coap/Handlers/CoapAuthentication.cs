using SkunkLab.Security.Identity;
using System.Diagnostics;
using System.Security;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public class CoapAuthentication
    {
        public static void EnsureAuthentication(CoapSession session, CoapMessage message, bool force = false)
        {            
            if(!session.IsAuthenticated || force)
            {
                CoapUri coapUri = new CoapUri(message.ResourceUri.ToString());

                //Trace.WriteLine(String.Format("Coap URI token type = {0}", coapUri.TokenType));
                //Trace.WriteLine(String.Format("Coap URI token = {0}", coapUri.SecurityToken));
                if (!session.Authenticate(coapUri.TokenType, coapUri.SecurityToken))
                {
                    Trace.WriteLine("Coap URI authentication failed.");
                    throw new SecurityException("CoAP session not authenticated.");
                }
                else
                {
                    IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, session.Config.Indexes);
                    session.Identity = decoder.Id;
                    session.Indexes = decoder.Indexes;
                    Trace.WriteLine("Coap URI authentication is successful");
                }
            }
        }
    }
}
