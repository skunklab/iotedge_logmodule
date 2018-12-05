using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Piraeus.Clients.Mqtt;
using SkunkLab.Channels;
using SkunkLab.Channels.Tcp;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.VirtualRtu.Adapters;
using SkunkLab.VirtualRtu.ModBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VirtualRtu.Common.Configuration;

namespace SkunkLab.Edge.Gateway.Mqtt
{
    public class MqttEdgeClient
    {
        
        
        public MqttEdgeClient(IssuedConfig config, string moduleConnectionString, CancellationToken token)
        {
            token.Register(CancelCallback);
            this.config = config;
            this.moduleConnectionString = moduleConnectionString;
        }

        private string moduleConnectionString;
        private ModuleClient moduleClient;
        private IssuedConfig config;
        private IChannel channel;
        private PiraeusMqttClient client;
        private CancellationTokenSource source;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                moduleClient = ModuleClient.CreateFromConnectionString(moduleConnectionString, TransportType.Mqtt);
                await moduleClient.SetInputMessageHandlerAsync("modbusOutput", ModBusOutput, moduleClient);
                await moduleClient.OpenAsync();                
            }
            catch(Exception ex)
            {
                Trace.TraceError("Module client failed to connect with '{0}'.", ex.Message);
                return false;
            }
            
            try
            {                
                source = new CancellationTokenSource();
                channel = ChannelFactory.Create(false, config.Hostname, config.Port, config.PskIdentity, Convert.FromBase64String(config.PSK), 1024, 1024000, source.Token);
                //channel = new TcpClientChannel2(config.Hostname, config.Port, config.PskIdentity, Convert.FromBase64String(config.PSK), 1024, 1024000, source.Token);
                client = new PiraeusMqttClient(new Protocols.Mqtt.MqttConfig(90.0), channel);
                ConnectAckCode code = await client.ConnectAsync(Guid.NewGuid().ToString(), "JWT", config.SecurityToken, 90);

                if(code != ConnectAckCode.ConnectionAccepted)
                {
                    throw new CommunicationsException(String.Format("MQTT connection failed with code '{0}'.", code));
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Edge client failed to connect with error '{0}'.", ex.Message);
                moduleClient = null;
                return false;
            }

            try
            {
                await client.SubscribeAsync(config.Resources.RtuInputResource, QualityOfServiceLevelType.AtMostOnce, SubscriptionMessageReceived);
            }
            catch(Exception ex)
            {
                Trace.TraceError("Subscription to resource failed with '{0}'.", ex.Message);
                return false;
            }

            return true;
        }


        private void CancelCallback()
        {
            //close everything
        }

        private void SubscriptionMessageReceived(string resourceUriString, string contextType, byte[] message)
        {
            try
            {
                MbapHeader header = MbapHeader.Decode(message);
                Trace.WriteLine(String.Format("{0} - Message received with unit id '{1}' and resource '{2}'.", DateTime.Now.ToString("dd-MM-yyyyThh-mm-ss.ffff"), header.UnitId, resourceUriString));
                
                if (config.Resources.UnitId != (int)header.UnitId)
                {
                    //invalid unit id -- do not process
                    Trace.TraceWarning("Unit Id '{0}' not found in map. Message cannot be forwarded to ModBus protocol adapter.", header.UnitId);
                    return;
                }
                else
                {
                    //forward to the ModBus protocol adapter
                    Task task = moduleClient.SendEventAsync("modbusInput", new Message(message));
                    Task.WhenAll(task);
                    Trace.WriteLine(String.Format("{0} - Message received with forwarded to protocol adapter with unit id '{1}' and resource '{2}'.", DateTime.Now.ToString("dd-MM-yyyyThh-mm-ss.ffff"), header.UnitId, resourceUriString));
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Faulted recevied message with error '{0}'.", ex.Message);
            } 
        }


        private async Task<MessageResponse> ModBusOutput(Message message, object userContext)
        {
            //input from RTU
            byte[] payload = message.GetBytes();
            MbapHeader header = MbapHeader.Decode(payload);
            if(config.Resources.UnitId == (int)header.UnitId)
            {
                //forward the message
                await client.PublishAsync(QualityOfServiceLevelType.AtMostOnce, config.Resources.RtuOutputResource, "application/octet-stream", payload);
                return MessageResponse.Completed;
            }
            else
            {
                Trace.TraceWarning("ModBus output has Unit ID mismatch.  Received '{0}' and expected '{1}'.", header.UnitId, config.Resources.UnitId);
                return MessageResponse.Abandoned;
            }
        }



    }
}
