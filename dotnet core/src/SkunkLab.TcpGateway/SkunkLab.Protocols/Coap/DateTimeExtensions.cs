

namespace SkunkLab.Protocols.Coap
{
    using System;
    using System.Text;
    using System.Xml;


    internal static class DateTimeExtensions
    {
        public static byte[] Convert(this DateTime? expires, string contentType)
        {
            MediaType media = MediaTypeConverter.ConvertToMediaType(contentType);

            switch (media)
            {
                case MediaType.TextPlain:
                    return expires.HasValue ? Encoding.UTF8.GetBytes(expires.Value.ToString()) : null;
                case MediaType.Json:
                    return expires.HasValue ? Encoding.UTF8.GetBytes(String.Format("{\"Expires\":\"{0}\"}", expires.Value.ToString())) : Encoding.UTF8.GetBytes(String.Format("{\"Expires\":\"\"}"));
                case MediaType.Xml:
                    return expires.HasValue ? Encoding.UTF8.GetBytes(String.Format("<Expires>{0}</Expires>", XmlConvert.ToString(expires.Value, XmlDateTimeSerializationMode.Utc))) : Encoding.UTF8.GetBytes("<Expires/>");
                case MediaType.OctetStream:
                    return expires.HasValue ? Encoding.UTF8.GetBytes(String.Format("Expires={0}", expires.Value.ToString())) : Encoding.UTF8.GetBytes(String.Format("Expires=\"\""));
                default: return null;
            }
        }
    }
}
