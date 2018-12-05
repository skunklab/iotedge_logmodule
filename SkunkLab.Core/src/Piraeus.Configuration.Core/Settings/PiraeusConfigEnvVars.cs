using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Configuration.Core.Settings
{
    public class PiraeusConfigEnvVars
    {
        public PiraeusConfigEnvVars()
        {
            websocketMaxIncomingMessageSize = 4194304;
            websocketReceiveLoopBufferSize = 8192;
            websocketSendBufferSize = 8192;
            websocketCloseTimeoutMilliseconds = 250.0;
            TCP_USE_LENGTH_PREFIX = true;
            TCP_CERT_AUTHN = false;
            tcpBlockSize = 8192;
            tcpMaxBufferSize = 4194304;
        }

        private int websocketMaxIncomingMessageSize;
        private int websocketReceiveLoopBufferSize;
        private int websocketSendBufferSize;
        private double websocketCloseTimeoutMilliseconds;
        private int tcpBlockSize;
        private readonly int tcpMaxBufferSize; 

        public int WEBSOCKET_MAX_INCOMING_MESSAGE_SIZE
        {
            get { return websocketMaxIncomingMessageSize; }
            set
            {
                if (value > 0)
                {
                    websocketMaxIncomingMessageSize = value;
                }
            }
        }

        public int WEBSOCKET_RECEIVE_LOOP_BUFFER_SIZE
        {
            get { return websocketReceiveLoopBufferSize; }
            set
            {
                if (value > 0)
                {
                    websocketReceiveLoopBufferSize = value;
                }
            }
        }

        public int WEBSOCKET_SEND_BUFFER_SIZE
        {
            get { return websocketSendBufferSize; }
            set
            {
                if (value > 0)
                {
                    websocketSendBufferSize = value;
                }
            }
        }

        public double WEBSOCKET_CLOSE_TIMEOUT_MILLISECONDS
        {
            get { return websocketCloseTimeoutMilliseconds; }
            set
            {
                if (value > 0)
                {
                    websocketCloseTimeoutMilliseconds = value;
                }
            }
        }

        public bool TCP_USE_LENGTH_PREFIX { get; set; }

        public bool TCP_CERT_AUTHN { get; set; }

        public int TCP_BLOCK_SIZE
        {
            get { return tcpBlockSize; }
            set
            {
                if (value > 0)
                {
                    tcpBlockSize = value;
                }
            }
        }

        public int TCP_MAX_BUFFER_SIZE
        {
            get { return tcpBlockSize; }
            set
            {
                if (value > 0)
                {
                    tcpBlockSize = value;
                }
            }
        }

        public string TCP_CERT_FILENAME { get; set; }

        public string TCP_CERT_PASSWORD { get; set; }

        public string PSK_IDENTITIES { get; set; }

        public string PSK_KEYS { get; set; }

        public string COAP_HOSTNAME { get; set; }
        public bool COAP_AUTO_RETRY { get; set; }

        public bool COAP_OBSERVE_OPTION { get; set; }

        public bool COAP_NORESPONSE_OPTION { get; set; }

        public double COAP_KEEP_ALIVE_SECONDS { get; set; }

        public double COAP_ACK_TIMEOUT_SECONDS { get; set; }

        public double COAP_ACK_RANDOM_FACTOR { get; set; }

        public int COAP_MAX_RETRANSMIT { get; set; }

        public double COAP_MAX_LATENCY_SECONDS { get; set; }
        
        public int COAP_NSTART { get; set; }

        public double COAP_DEFAULT_LEISURE { get; set; }

        public double COAP_PROBING_RATE { get; set; }

        private readonly string coapHostname;
        private readonly bool coapAutoRetry;
        private readonly bool coapObserveOption;
        private readonly bool coapNoReponseOption;
        private readonly double coapKeepAliveSeconds;
        private readonly double coapAckTimeoutSeconds;
        private readonly double coapAckRandomFactor;
        private readonly int coapMaxRetransmit;
        private readonly double coapMaxLatencySeconds;
        private readonly int coapNStart;
        private readonly double coapDefaultLeisure;
        private readonly double coapProbingRate;

        //string coapAuthority = System.Environment.GetEnvironmentVariable("COAP_HOSTNAME");
        //            bool coapAutoRetry = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("COAP_AUTO_RETRY") ?? "false");
        //            bool observeOption = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("COAP_OBSERVE_OPTION") ?? "true");
        //            bool noResponseOption = Convert.ToBoolean(System.Environment.GetEnvironmentVariable("COAP_NORESPONSE_OPTION") ?? "true");
        //            double coapKeepAlive = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_KEEP_ALIVE_SECONDS") ?? "180.0");
        //            double coapAckTimeout = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_ACK_TIMEOUT_SECONDS") ?? "2.0");
        //            double coapAckRandFactor = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_ACK_RANDOM_FACTOR") ?? "1.5");
        //            int coapMaxRetransmit = Convert.ToInt32(System.Environment.GetEnvironmentVariable("COAP_MAX_RETRANSMIT") ?? "4");
        //            double coapMaxLatency = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_MAX_LATENCY_SECONDS") ?? "100.0");
        //            int nstart = Convert.ToInt32(System.Environment.GetEnvironmentVariable("COAP_NSTART") ?? "1");
        //            double coapLeisure = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_DEFAULT_LEISURE") ?? "4.0");
        //            double probeRate = Convert.ToDouble(System.Environment.GetEnvironmentVariable("COAP_PROBING_RATE") ?? "1.0");


    }
}
