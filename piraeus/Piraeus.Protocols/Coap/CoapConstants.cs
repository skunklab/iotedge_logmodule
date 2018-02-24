

namespace Piraeus.Protocols.Coap
{
    using System;
    public static class CoapConstants
    {
        public class Timeouts
        {
            public static TimeSpan MaxTransmitSpan = new TimeSpan(0, 0, 45);
            public static TimeSpan MaxTransmitWait = new TimeSpan(0, 0, 93);
            public static TimeSpan MaxLatency = new TimeSpan(0, 0, 100);
            public static TimeSpan ProcessingDelay = new TimeSpan(0, 0, 2);
            public static TimeSpan MaxRtt = new TimeSpan(0, 0, 202);
            public static TimeSpan ExchangeLifetime = new TimeSpan(0, 0, 247);
            public static TimeSpan NonLifetime = new TimeSpan(0, 0, 145);
            public static TimeSpan AckTimeout = new TimeSpan(0, 0, 2);
            public const decimal AckRandomFactor = 1.5m;
            public const int MaxRetransmit = 4;
            public const int NStart = 1;
            public static TimeSpan Default_Leisure = new TimeSpan(0, 0, 5);
            public const int ProbingRate = 1;

        }
    }
}
