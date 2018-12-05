//using Microsoft.Azure.Devices.Client;
//using Microsoft.Azure.Devices.Shared;
//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;

//namespace SkunkLab.Edge.Gateway.Twins
//{
//    public class EdgeTwin
//    {

//        private TransportType transport;
//        private ModuleClient client;
//        public event System.EventHandler<TwinMessageEventArgs> OnReceive;
//        private string connectionString;
//        private Twin twin;

//        public EdgeTwin(TransportType transport)
//        {
//            this.transport = transport;
//        }

//        public EdgeTwin(string moduleConnectionString, TransportType transport)
//        {
//            connectionString = moduleConnectionString;
//        }

//        public async Task ConnectAsync()
//        {
//            ExponentialBackoff policy = new ExponentialBackoff(5, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20));

//            if (connectionString == null)
//            {
//                client = await ModuleClient.CreateFromEnvironmentAsync(transport);
//            }
//            else
//            {
//                client = ModuleClient.CreateFromConnectionString(connectionString);
//            }

//            client.SetRetryPolicy(policy);            


//            twin = await client.GetTwinAsync();
//            TwinCollection collection = twin.Properties.Desired;
            

//            if (collection.Contains("luss") && collection.Contains("serviceUrl"))
//            {
//                string luss = collection["luss"];
//                string serviceUrl = collection["serviceUrl"];
//                OnReceive?.Invoke(this, new TwinMessageEventArgs(luss, serviceUrl));
//            }

//            await client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);
//        }

//        public async Task CloseAsync()
//        {            
//            if(client != null)
//            {                
//                await client.CloseAsync();
//            }

//            client = null;
//        }


//        private Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
//        {
//            try
//            {
//                string luss = desiredProperties["luss"];
//                string url = desiredProperties["serviceUrl"];
//                OnReceive?.Invoke(this, new TwinMessageEventArgs(luss, url));
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Desired properties for module update error.");
//                Trace.TraceError(ex.Message);
//            }

//            return Task.CompletedTask;
//        }


//        public async Task ReportAsync()
//        {
//            try
//            {
//                TwinCollection collection = new TwinCollection();
//                collection["updated"] = DateTime.UtcNow;
//                await client.UpdateReportedPropertiesAsync(collection);
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Module twin report properties update failed.");
//                Trace.TraceError("Module twin update failed with '{0}'.", ex.Message);
//            }
//        }
//    }
//}
