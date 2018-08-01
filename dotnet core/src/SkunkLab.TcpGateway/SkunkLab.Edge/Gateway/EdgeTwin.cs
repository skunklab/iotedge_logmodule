using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;
using SkunkLab.Edge.Gateway.Mqtt;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SkunkLab.Edge.Gateway
{
    public class EdgeTwin
    {
        private string deviceConnectionString = "HostName=virtualrtu.azure-devices.net;DeviceId=rtu1;SharedAccessKey=k4N3czIvUNyqqcnhhASbWOUOMtJitUnQfL0qRrE/fj8=";
        private DeviceClient client = null;

        public EdgeTwin()
        {
            deviceConnectionString = System.Environment.GetEnvironmentVariable("DEVICE_CONNECTION_STRING");

            if (!string.IsNullOrEmpty(deviceConnectionString))
            {
                client = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);
            }
        }

        public async Task<EdgeConfig> ConnectAsync()
        {
            if (client == null)
            {
                return null;
            }

            EdgeConfig config = null;

            try
            {
                Twin twin = await client.GetTwinAsync();

                TwinCollection collection = twin.Properties.Desired;
                if (collection.Contains("edgeConfig"))
                {
                    JObject obj = collection["edgeConfig"];
                    config = obj.ToObject<EdgeConfig>();
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Twin connected failed with '{0}'.", ex.Message);
            }

            return config;
        }


        public async Task ReportAsync(EdgeConfig config)
        {
            try
            {
                TwinCollection collection = new TwinCollection();
                collection["edgeConfig"] = config;
                await client.UpdateReportedPropertiesAsync(collection);
            }
            catch(Exception ex)
            {
                Trace.TraceError("Twin update failed with '{0}'.", ex.Message);
            }
        }

    }
}
