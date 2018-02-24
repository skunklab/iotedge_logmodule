
namespace Piraeus.Protocols.Mqtt
{
    public enum ConnectAckCode
    {        
        ConnectionAccepted = 0,
        UnacceptableProtocolVersion = 1,
        IdentifierRejected = 2,
        ServerUnavailable = 3,
        BadUsernameOrPassword = 4,
        NotAuthorized = 5
    }
}
