
namespace SkunkLab.Protocols.Mqtt
{
    public enum QualityOfServiceLevelType
    {
        AtMostOnce = 0,
        AtLeastOnce = 1,
        ExactlyOnce = 2,
        Failure = 128
    }
}
