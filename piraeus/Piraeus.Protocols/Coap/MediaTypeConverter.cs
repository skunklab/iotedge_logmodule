


namespace Piraeus.Protocols.Coap
{
    using System;
    using System.Globalization;

    public static class MediaTypeConverter
    {
        public static MediaType ConvertToMediaType(string contentType)
        {
            if(string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException("contentType");
            }

            string lower = contentType.ToLower(CultureInfo.InvariantCulture);

            if(lower == "text/plain")
            {
                return MediaType.TextPlain;
            }
            else if (lower == "application/json" || lower == "text/json")
            {
                return MediaType.Json;
            }
            else if( lower == "application/xml" || lower == "text/xml")
            {
                return MediaType.Xml;
            }
            else if(lower == "application/octet-stream")
            {
                return MediaType.OctetStream;
            }
            else
            {
                throw new UnsupportedMediaTypeException(String.Format("Content-Type of '{0}' is not supported.", contentType)); 
            }
        }


        public static string ConvertFromMediaType(MediaType mediaType)
        {
            if(mediaType == MediaType.TextPlain)
            {
                return "text/plain";
            }
            else if(mediaType == MediaType.Json)
            {
                return "application/json";
            }
            else if(mediaType == MediaType.Xml)
            {
                return "application/xml";
            }
            else if(mediaType == MediaType.OctetStream)
            {
                return "application/octet-stream";
            }
            else
            {
                throw new UnsupportedMediaTypeException(String.Format("Media type of '{0}' is not supported.", mediaType.ToString()));
            }
        }


    }
}
