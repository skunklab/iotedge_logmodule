using Piraeus.Core;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.GrainInterfaces;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Piraeus.Grains.Notifications
{
    public class RestWebServiceSink : EventSink
    {
        public RestWebServiceSink(SubscriptionMetadata metadata)
            : base(metadata)
        {
            auditor = new Auditor();
            tokenType = metadata.TokenType;
            symmetricKey = metadata.SymmetricKey;

            Uri uri = new Uri(metadata.NotifyAddress);


            NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
            issuer = nvc["issuer"];
            audience = nvc["audience"];
            nvc.Remove("issuer");
            nvc.Remove("audience");

            string uriString = nvc.Count == 0 ? String.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Authority, uri.LocalPath) :
                String.Format("{0}{1}{2}{3}?", uri.Scheme, Uri.SchemeDelimiter, uri.Authority, uri.LocalPath);

            StringBuilder builder = new StringBuilder();
            builder.Append(uriString);
            for (int i = 0; i < nvc.Count; i++)
            {
                string key = nvc.GetKey(i);
                string value = nvc[key];
                builder.Append(String.Format("{0}={1}", key, value));
                if (i < nvc.Count - 1)
                    builder.Append("&");

            }

            address = builder.ToString();
            
            SetCertificateAsync().Ignore();

            SetClaimsAsync().Ignore();
        }

        private Piraeus.Core.Metadata.SecurityTokenType? tokenType;
        private string symmetricKey;
        private string issuer;
        private string audience;
        private string address;
        private DateTime tokenExpiration;
        private string token;
        private X509Certificate2 certificate;
        private List<Claim> claims;
        private Auditor auditor;


        private async Task SetCertificateAsync()
        {            
            certificate = await GraphManager.GetServiceIdentityCertificateAsync();
        }

        private async Task SetClaimsAsync()
        {
            tokenExpiration = DateTime.UtcNow.AddMinutes(19.0);
            claims = await GraphManager.GetServiceIdentityClaimsAsync();
        }

        private void SetSecurityToken(HttpWebRequest request)
        {
            if (!metadata.TokenType.HasValue || metadata.TokenType.Value == Piraeus.Core.Metadata.SecurityTokenType.None)
                return;

            if (metadata.TokenType.Value == Piraeus.Core.Metadata.SecurityTokenType.X509)
            {
                if (certificate == null)
                {
                    throw new InvalidOperationException("X509 client certificates not available to use for REST call.");
                }
                else
                {
                    request.ClientCertificates.Add(certificate);
                }
            }
            else if (metadata.TokenType.Value == Piraeus.Core.Metadata.SecurityTokenType.Jwt)
            {
                JsonWebToken jwt = new JsonWebToken(metadata.SymmetricKey, claims, 20.0, issuer, audience);
                token = jwt.ToString();

                request.Headers.Add("Authorization", String.Format("Bearer {0}", token.ToString()));

                request.Headers.Add("Authorization", String.Format("Bearer {0}", token));
            }
            else
            {
                throw new InvalidOperationException("No security token type resolved.");
            }
        }

        public override async Task SendAsync(EventMessage message)
        {
            AuditRecord record = null;
            HttpWebRequest request = null;
            byte[] payload = null;
            try
            {
                payload = GetPayload(message);
                if (payload == null)
                {
                    Trace.TraceWarning("Subscription {0} could not write to web service sink because payload was either null or unknown protocol type.");
                    return;
                }

                try
                {
                    request = HttpWebRequest.Create(address) as HttpWebRequest;
                    request.ContentType = message.ContentType;
                    request.Method = "POST";

                    SetSecurityToken(request);

                    request.ContentLength = payload.Length;
                    Stream stream = await request.GetRequestStreamAsync();
                    await stream.WriteAsync(payload, 0, payload.Length);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("REST event sink subscription {0} could set request; error {1} ", metadata.SubscriptionUriString, ex.Message);
                    record = new AuditRecord(message.MessageId, address, "WebService", "HTTP", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
                }

                try
                {
                    using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                    {
                        if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                        {
                            Trace.TraceInformation("Rest request is success.");
                            record = new AuditRecord(message.MessageId, address, "WebService", "HTTP", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);

                        }
                        else
                        {
                            Trace.TraceInformation("Rest request returned an expected status code.");
                            record = new AuditRecord(message.MessageId, address, "WebService", "HTTP", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, String.Format("Rest request returned an expected status code {0}", response.StatusCode));
                        }
                    }
                }
                catch (WebException we)
                {
                    string faultMessage = String.Format("subscription '{0}' with status code '{1}' and error message '{2}'", metadata.SubscriptionUriString, we.Status.ToString(), we.Message);
                    Trace.TraceError(faultMessage);
                    record = new AuditRecord(message.MessageId, address, "WebService", "HTTP", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, we.Message);                   
                }
            }
            catch(Exception ex)
            {
                record = new AuditRecord(message.MessageId, address, "WebService", "HTTP", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
            }
            finally
            {
                if(message.Audit && record != null && auditor.CanAudit)
                {
                    await auditor.WriteAuditRecordAsync(record);
                }
            }
        }

        private byte[] GetPayload(EventMessage message)
        {
            switch (message.Protocol)
            {
                case ProtocolType.COAP:
                    CoapMessage coap = CoapMessage.DecodeMessage(message.Message);
                    return coap.Payload;
                case ProtocolType.MQTT:
                    MqttMessage mqtt = MqttMessage.DecodeMessage(message.Message);
                    return mqtt.Payload;
                case ProtocolType.REST:
                    return message.Message;
                case ProtocolType.WSN:
                    return message.Message;
                default:
                    return null;
            }
        }
        
    }
}
