using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PiraeusClientModule.Clients.ModBus
{
    [Serializable]
    [JsonObject]
    public class ModBusClientConfig
    {
        public ModBusClientConfig(IPAddress remoteAddress, string requestResource, string responseResource, string securityToken, 
                                string pskIdentity, byte[] psk, int tcpBlockSize = 1024, int tcpMaxBufferSize = 10240, 
                                int mqttPort = 8883, bool keyChannelOpen = true, TimeSpan? channelOpenDelay = null)
        {
            this.RemoteAddress = remoteAddress;
            this.ModBusRequestResource = requestResource;
            this.ModBusResponseResource = responseResource;
            this.SecurityToken = securityToken;
            this.PskIdentity = pskIdentity;
            this.PSK = psk;
            this.TcpBlockSize = tcpBlockSize;
            this.TcpMaxBufferSize = tcpMaxBufferSize;
            this.MqttPort = mqttPort;
            this.KeepChannelOpen = keyChannelOpen;
            this.ChannelOpenDelay = channelOpenDelay.HasValue ? channelOpenDelay.Value : TimeSpan.FromSeconds(30.0);
        }


        public static ModBusClientConfig Load()
        {
            string hostname = System.Environment.GetEnvironmentVariable("HOSTNAME");
            this.RemoteAddress = Dns.GetHostEntry(hostname).AddressList[0];

            System.Environment.GetEnvironmentVariable("REQUEST_RESOURCE");
            System.Environment.GetEnvironmentVariable("RESPONSE_RESOURCE");
            System.Environment.GetEnvironmentVariable("SECURITY_KEY");
            System.Environment.GetEnvironmentVariable("PSK_IDENTITY");
            System.Environment.GetEnvironmentVariable("PSK");
            System.Environment.GetEnvironmentVariable("TCP_BLOCK_SIZE");
            System.Environment.GetEnvironmentVariable("TCP_MAX_BUFFER_SIZE");
            System.Environment.GetEnvironmentVariable("MQTT_PORT");
            System.Environment.GetEnvironmentVariable("KEY_CHANNEL_OPEN");
            System.Environment.GetEnvironmentVariable("CHANNEL_OPEN_DELAY");

        }

        public ModBusClientConfig(string hostname, string requestResource, string responseResource, string securityToken, string pskIdentity, byte[] psk, int tcpBlockSize = 1024, int tcpMaxBufferSize = 10240, int mqttPort = 8883, bool keyChannelOpen = true)
        : this(Dns.GetHostEntry(hostname).AddressList[0], requestResource, responseResource, securityToken, pskIdentity, psk, tcpBlockSize, tcpMaxBufferSize, mqttPort, keyChannelOpen)
        {
        }

        public ModBusClientConfig()
        {

        }


        [JsonProperty("keepChannelOpen")]
        public bool KeepChannelOpen { get; set; }

        [JsonProperty("channelOpenDelay")]
        public TimeSpan ChannelOpenDelay { get; set; }

        [JsonProperty("securityToken")]
        public string SecurityToken { get; set; }

        [JsonProperty("modBusRequestResource")]
        public string ModBusRequestResource { get; set; }

        [JsonProperty("modBusResponseResource")]
        public string ModBusResponseResource { get; set; }

        [JsonProperty("psk")]
        public byte[] PSK { get; set; }

        [JsonProperty("pskIdentity")]
        public string PskIdentity { get; set; }

        [JsonProperty("tcpBlockSize")]
        public int TcpBlockSize { get; set; }

        [JsonProperty("tcpMaxBufferSize")]
        public int TcpMaxBufferSize { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("mqttPort")]
        public int MqttPort { get; set; }

        [JsonProperty("remoteAddress")]
        public IPAddress RemoteAddress { get; set; }


    }
}
