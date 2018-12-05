using Newtonsoft.Json;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class ServiceSecurity
    {
        public ServiceSecurity()
        {

        }
        public ServiceSecurity(string filename, string password)
        {
            Filename = filename;
            Password = password;
        }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        public X509Certificate2 GetCertificate()
        {
            if(HasCertificate())
            {
                return new X509Certificate2(Filename, Password);
            }
            else
            {
                return null;
            }
        }

        public bool HasCertificate()
        {
            return !string.IsNullOrEmpty(Filename) && !string.IsNullOrEmpty(Password);
        }
    }
}
