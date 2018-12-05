using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VirtualRtu.Common.Configuration;

namespace SkunkLab.Edge.Gateway.Modules
{
    public class EdgeModuleClient
    {       
        private Microsoft.Azure.Devices.Client.ModuleClient moduleClient;
        private Twin twin;
        private TransportType transport;
        private string inputRoute;
        private string outputRoute;
        
        public event System.EventHandler<ModuleMessageEventArgs> OnReceive;
        public event System.EventHandler<TwinMessageEventArgs> OnTwinReceive;
        

        public EdgeModuleClient(string inputRoute, string outputRoute)
            : this(inputRoute, outputRoute, TransportType.Mqtt)
        {
        }

        public EdgeModuleClient(string inputRoute, string outputRoute, TransportType transport)
        {
            this.inputRoute = inputRoute;
            this.outputRoute = outputRoute;
            this.transport = transport;
           
        }

        public EdgeModuleClient(string inputRoute, string outputRoute, string moduleConnectionString, TransportType transport)
        {
            moduleClient = Microsoft.Azure.Devices.Client.ModuleClient.CreateFromConnectionString(moduleConnectionString, transport);
            this.inputRoute = inputRoute;
            this.outputRoute = outputRoute;
            
        }

        public async Task ConnectAsync()
        {
            //ExponentialBackoff policy = new ExponentialBackoff(5, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20));
            NoRetry policy = new NoRetry();
            
            if (moduleClient == null)
            {
                //ITransportSettings[] settings =
                //{
                //    new MqttTransportSettings(TransportType.Mqtt_Tcp_Only)
                //    {
                //       RemoteCertificateValidationCallback =(sender, certificate, chain, sslPolicyErrors) => true }
                // };


                moduleClient = await Microsoft.Azure.Devices.Client.ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt);
                moduleClient.SetRetryPolicy(policy);
                //moduleClient.OperationTimeoutInMilliseconds = 10000;
            }

            var context = moduleClient;
            await moduleClient.SetInputMessageHandlerAsync(inputRoute, InputAsync, context);
            await moduleClient.SetMessageHandlerAsync(new MessageHandler(DefaultHandler), context);
            await moduleClient.OpenAsync();

        }

        public async Task<MessageResponse> DefaultHandler(Message message, object context)
        {
            Console.WriteLine("Default Handler");
            try
            {
                await moduleClient.CompleteAsync(message);
                Console.WriteLine("Message received and 'completed' from Edge Hub (Default handler).");
                byte[] payload = message.GetBytes();
                OnReceive?.Invoke(this, new ModuleMessageEventArgs(payload));
            }
            catch
            {
                Console.WriteLine("Warning: Received message from IoT Hub, but fault forces abandon message.");
            }

            Console.WriteLine("Responding with message 'Completed'");
            return MessageResponse.Completed;
        }

        public async Task StartTwinAsync()
        {
            twin = await moduleClient.GetTwinAsync();
            TwinCollection collection = twin.Properties.Desired;


            if (collection.Contains("luss") && collection.Contains("serviceUrl"))
            {
                string luss = collection["luss"];
                string serviceUrl = collection["serviceUrl"];
                OnTwinReceive?.Invoke(this, new TwinMessageEventArgs(luss, serviceUrl));
            }

            await moduleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);
        }

        public async Task SendAsync(byte[] message)
        {
            try
            {                
                await moduleClient.SendEventAsync(outputRoute, new Message(message));
                Console.WriteLine("Message sent to route - '{0}'", outputRoute);
            }
            catch
            {
                Console.WriteLine("Fault during message send to route - '{0}'", outputRoute);
            }
        }


        

        private async Task<MessageResponse> InputAsync(Message message, object userContext)
        {           

            try
            {
                ModuleClient mc = (ModuleClient)userContext;
                await mc.CompleteAsync(message);
                Console.WriteLine("Message received from Edge Hub and 'completed'");
                byte[] payload = message.GetBytes();
                OnReceive?.Invoke(this, new ModuleMessageEventArgs(payload));                
            }
            catch
            {
                Console.WriteLine("Warning: Received message from IoT Hub, but fault forces abandon message.");
            }

            return MessageResponse.Completed;
        }

        private Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {           
            try
            {
                string luss = desiredProperties["luss"];
                string url = desiredProperties["serviceUrl"];
                OnTwinReceive?.Invoke(this, new TwinMessageEventArgs(luss, url));
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Desired properties for module update error.");
                Trace.TraceError(ex.Message);
            }

            return Task.CompletedTask;
        }

        public async Task ReportPropertiesAsync()
        {
            try
            {
                TwinCollection collection = new TwinCollection();
                collection["updated"] = DateTime.UtcNow;
                await moduleClient.UpdateReportedPropertiesAsync(collection);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Module twin report properties update failed.");
                Trace.TraceError("Module twin update failed with '{0}'.", ex.Message);
            }
        }

    }
}
