using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace ProvisioningService
{
    internal static class Serializer
    {
        public static byte[] Serialize<T>(string contentType, T body)
        {
            byte[] result = null;
            if (contentType == "application/json")
            {
                string jsonString = JsonConvert.SerializeObject(body);
                result = Encoding.UTF8.GetBytes(jsonString);
            }
            else if (contentType == "application/xml" || contentType == "text/xml")
            {
                XmlWriterSettings settings = new XmlWriterSettings() { OmitXmlDeclaration = true };
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream())
                {
                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
                        xs.Serialize(writer, body);
                    }
                    //xs.Serialize(stream, body);
                    stream.Position = 0;
                    result = stream.ToArray();
                }
            }
            else if (contentType == "text/plain")
            {
                result = Encoding.UTF8.GetBytes(Convert.ToString(body));
            }
            else if (contentType == "application/octet-stream")
            {
                result = body as byte[];
            }
            else
            {
                throw new InvalidCastException("contentType");
            }

            return result;
        }

        public static T Deserialize<T>(string contentType, byte[] body)
        {
            T result = default(T);
            if (contentType == "application/json")
            {
                string jsonString = Encoding.UTF8.GetString(body);
                result = JsonConvert.DeserializeObject<T>(jsonString);
            }
            else if (contentType == "application/xml" || contentType == "text/xml")
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream(body))
                {
                    stream.Position = 0;
                    result = (T)xs.Deserialize(stream);
                }
            }
            else if (contentType == "text/plain")
            {
                result = (T)Convert.ChangeType(Encoding.UTF8.GetString(body), typeof(T));
            }
            else if (contentType == "application/octet-stream")
            {
                result = (T)Convert.ChangeType(body, typeof(T));
            }
            else
            {
                throw new InvalidCastException("contentType");
            }

            return result;
        }
    }
}
