using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.Edge.Gateway.Mqtt
{
    public class MqttReceiveEventArgs : EventArgs
    {
        public MqttReceiveEventArgs(string resourceUriString, string contentType, byte[] message)
        {
            ResourceUriString = resourceUriString;
            ContentType = contentType;
            Message = message;
        }

        public string ResourceUriString { get; internal set; }

        public string ContentType { get; internal set; }

        public byte[] Message { get; internal set; }
    }
}
