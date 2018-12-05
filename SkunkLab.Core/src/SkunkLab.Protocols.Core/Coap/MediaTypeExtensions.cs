
namespace SkunkLab.Protocols.Coap
{
    using System;
    using System.Globalization;

    public static class MediaTypeExtensions
    {
        public static string ConvertToContentType(this MediaType mediaType)
        {            
            switch(mediaType)
            {
                case MediaType.Xml:
                    return "text/xml";
                case MediaType.TextPlain:
                    return "text/plain";
                case MediaType.OctetStream:
                    return "application/octet-stream";
                case MediaType.Json:
                    return "application/json";
                default:
                    throw new InvalidCastException("MediaType content");
            }
        }

        public static MediaType ConvertFromContentType(this MediaType mediaType, string contentType)
        {
            switch(contentType.ToLower(CultureInfo.InvariantCulture))
            {
                case "text/xml":
                    return MediaType.Xml;
                case "application/xml":
                    return MediaType.Xml;
                case "text/plain":
                    return MediaType.TextPlain;
                case "application/octet-stream":
                    return MediaType.OctetStream;
                case "text/json":
                    return MediaType.Json;
                case "application/json":
                    return MediaType.Json;
                default: throw new InvalidCastException("contentType");
            }
        }

        
    }
}
