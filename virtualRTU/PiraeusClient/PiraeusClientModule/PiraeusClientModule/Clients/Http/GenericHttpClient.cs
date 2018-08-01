using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PiraeusClientModule.Clients.Http
{
    public class GenericHttpClient
    {
        public GenericHttpClient(string requestUri)
        {
            this.requestUri = requestUri;
        }

        private string requestUri;

        public  async Task<byte[]> SendAsync(byte[] payload = null)
        {
            byte[] msg = null;

            try
            {
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                HttpContent content = new ByteArrayContent(payload);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                HttpResponseMessage output = await client.PostAsync(requestUri, content);
                if(output.StatusCode != HttpStatusCode.OK)
                {
                    Trace.TraceWarning("Invalid HTTP status code '{0}' - '{1}'", output.StatusCode, DateTime.Now);
                }

                msg = await output.Content.ReadAsByteArrayAsync();
            }
            catch(WebException wex)
            {
                Trace.TraceWarning("Web exception sending to ModBus Protocol Adpater '{0}'", DateTime.Now);
                Trace.TraceError(wex.Message);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Http client exception sending to ModBus Protocol Adpater '{0}'", DateTime.Now);
                Trace.TraceError(ex.Message);
            }
            finally
            {
                return msg;
            }
        }
            

    }
}
