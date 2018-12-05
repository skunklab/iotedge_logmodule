using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class TcpSettings
    {
        public TcpSettings()
        {

        }
        public TcpSettings(bool useLengthPrefix, int blockSize, int maxBufferSize, int[] ports, string hostname, bool authenticate = false, 
                            string certificateFilename = null, string certificatePassword = null, Dictionary<string, byte[]> presharedKeys = null)
        {
            
            UseLengthPrefix = useLengthPrefix;
            BlockSize = blockSize;
            MaxBufferSize = maxBufferSize;
            Ports = ports;
            Hostname = hostname;
            Authenticate = authenticate;
            Ports = ports;
            CertificateFilename = certificateFilename;
            CertificatePassword = certificatePassword;
            psks =  presharedKeys;
            
        }

        private Dictionary<string, byte[]> psks;
        private Dictionary<string, string> propertyPsks;
       
        [JsonProperty("useLengthPrefix")]
        public bool UseLengthPrefix { get; set; }

        /// <summary>
        /// Authenticates a certificate used.
        /// </summary>
        [JsonProperty("authenticate")]
        public bool Authenticate { get; set; }

        [JsonProperty("blockSize")]
        public int BlockSize { get; set; }

        [JsonProperty("maxBufferSize")]
        public int MaxBufferSize { get; set; }

        [JsonProperty("ports")]
        public int[] Ports { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// Certificate filename, i.e., folder/subfolder/filename.pfx
        /// </summary>
        [JsonProperty("certificateFilename")]
        public string CertificateFilename { get; set; }

        /// <summary>
        /// Certificate password
        /// </summary>
        [JsonProperty("certificatePassword")]
        public string CertificatePassword { get; set; }

        [JsonProperty("presharedKeys")]
        public Dictionary<string, string> PresharedKeys 
        {
            get { return propertyPsks; }
            set
            {
                if(value != null)
                {
                    psks = new Dictionary<string, byte[]>();
                    Dictionary<string, string>.Enumerator en = value.GetEnumerator();
                    while(en.MoveNext())
                    {
                        psks.Add(en.Current.Key, Convert.FromBase64String(en.Current.Value));
                    }
                }

                propertyPsks = value;
            }
        }

        public Dictionary<string, byte[]> GetPskClone()
        {            
            return DeepClone<Dictionary<string, byte[]>>(psks);
        }

        public X509Certificate2 GetCertificate()
        {
            if (HasCertificate())
            {
                if (string.IsNullOrEmpty(CertificatePassword))
                {
                    return new X509Certificate2(CertificateFilename);
                }
                else
                {
                    return new X509Certificate2(CertificateFilename, CertificatePassword);
                }
            }
            else
            {
                return null;
            }
        }

        public bool HasCertificate()
        {
            return !string.IsNullOrEmpty(CertificateFilename);
        }

        private T DeepClone<T>(T obj)
        {
            if(obj == null)
            {
                return default(T);
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

    }
}
