using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Sample.Mqtt.Clients
{
    public class MyMessage
    {
        public long Ticks { get; set; }

        public string Payload { get; set; }

        public string GetHash()
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(Payload)));
            }
        }
    }
}
