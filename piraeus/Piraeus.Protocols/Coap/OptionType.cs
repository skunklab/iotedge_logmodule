
namespace Piraeus.Protocols.Coap
{
    public enum OptionType
    {
        IfMatch = 1,        //opaque
        UriHost = 3,        //string
        ETag = 4,           //opaque
        IfNoneMatch = 5,    //empty
        UriPort = 7,        //uint
        LocationPath = 8,   //string
        UriPath = 11,       //string
        ContentFormat = 12, //uint
        MaxAge = 14,        //uint
        UriQuery = 15,      //string
        Accept = 17,        //uint
        LocationQuery = 20, //string
        ProxyUri = 35,      //string
        ProxyScheme = 39,   //string
        Size1 = 60          //uint

        //opaque = 1,4
        //empty = 5
        //string = 2,8,11,15,20,35,39
        //uint = 7,12,14,17,60
      
    }
}
