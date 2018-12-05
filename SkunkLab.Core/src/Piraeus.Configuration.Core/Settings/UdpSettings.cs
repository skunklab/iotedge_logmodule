using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class UdpSettings
    {
        public UdpSettings()
        {
        }

        public UdpSettings(string hostname, int[] ports)
        {
            Hostname = hostname;
            Ports = ports;
        }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }


        [JsonProperty("ports")]
        public int[] Ports { get; set; }
    }
}
