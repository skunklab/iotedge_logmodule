using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionWrapper
{
    public static class Wrapper
    {
        [FunctionName("Wrapper")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, null, Route = "{*url}")]HttpRequestMessage req, TraceWriter log, ExecutionContext context)
        {
            log.Info("Starting Function");

            // TODO: Add a project reference to your WebAPI project, and add the relevant using statement
            using (var host = new WebApiFunctionHost.WebApiFunctionHost(context, WebApiConfig.Register))
            {
                return await host.HandleAsync(req);
            }
        }
    }
}