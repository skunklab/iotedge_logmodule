
namespace SkunkLab.Protocols.Mqtt
{
    internal static class Constants
    {

        internal static class MessageTypes
        {
            public const byte Connect = 0x01;
            public const byte Connack = 0x02;
            public const byte Publish = 0x03;
            public const byte Puback = 0x04;
            public const byte Pubrec = 0x05;
            public const byte Pubrel = 0x06;
            public const byte Pubcomp = 0x07;
            public const byte Subscribe = 0x08;
            public const byte Suback = 0x09;
            public const byte Unsuback = 0x0A;
            public const byte Pingreq = 0x0B;
            public const byte Pingresp = 0x0C;
            public const byte Disconnect = 0x0E;
       
        }
        internal static class Header
        {
            public const byte MessageTypeOffset = 0x04;
            public const byte DupFlagOffset = 0x03;
            public const byte QosLevelOffset = 0x01;
        }

        
    }
}
