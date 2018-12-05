using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using SkunkLab.Edge.Gateway.Mqtt;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SkunkLab.Edge.Gateway
{
    public class EdgeTwin
    {
        public event System.EventHandler<ModuleConfigurationEventArgs> OnConfiguration;
        private ModuleClient client;
        private Twin twin;
        public EdgeTwin()
        {            
        }


        public async Task CloseAsync()
        {
            if(client != null)
            {
                await client.CloseAsync();
            }
        }

        public async Task ConnectAsync(string moduleConnectionString)
        {           
            if (moduleConnectionString == null)
            {
                throw new ArgumentNullException("moduleConnectionString");
            }

            try
            {
                client = ModuleClient.CreateFromConnectionString(moduleConnectionString, TransportType.Mqtt);
                twin = await client.GetTwinAsync();

                TwinCollection collection = twin.Properties.Desired;
                EdgeConfig config = new EdgeConfig() { LUSS = collection["luss"], ServiceUrl = collection["serviceUrl"] };

                OnConfiguration?.Invoke(this, new ModuleConfigurationEventArgs(config));
                await client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            }
            catch (Exception ex)
            {
                Trace.TraceError("Twin connected failed with '{0}'.", ex.Message);
            }
        }

        public Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            //receiving an update from reconfiguration

            //raise and event for possible reconfiguration
            try
            {
                string luss = desiredProperties["luss"];
                string url = desiredProperties["serviceUrl"];
                EdgeConfig config = new EdgeConfig() { LUSS = luss, ServiceUrl = url };
                OnConfiguration?.Invoke(this, new ModuleConfigurationEventArgs(config));
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Desired properties for module update error.");
                Trace.TraceError(ex.Message);
            }            

            return Task.CompletedTask;
        }


        public async Task ReportAsync(bool complete)
        {
            try
            {
                TwinCollection collection = new TwinCollection();
                collection["configurationComplete"] = complete;
                await client.UpdateReportedPropertiesAsync(collection);
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Module twin report properties update failed.");
                Trace.TraceError("Module twin update failed with '{0}'.", ex.Message);
            }
        }

    }
}
