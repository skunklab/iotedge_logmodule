using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FieldGatewayMicroservice.Connections
{
    public class IPHelper
    {

        static IPHelper()
        {
            queue = new Queue<byte[]>();
            IPHostEntry entry = Dns.GetHostEntry("mbpa");
            string ipAddressString = null;

            foreach (var address in entry.AddressList)
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (address.ToString().Contains("172"))
                    {
                        ipAddressString = address.ToString();
                        break;
                    }

                }
            }

            if (ipAddressString != null)
            {
                requestUrl = String.Format("http://{0}:8889/api/rtuinput", ipAddressString);
                Console.WriteLine("REQUEST URL = '{0}'", requestUrl);
            }
            else
            {
                Console.WriteLine("NO IP ADDRESS FOUND");
            }
        }

        private static Queue<byte[]> queue;
        private static string requestUrl;

        public static Queue<byte[]> Queue
        {
            get { return queue; }
        }
        public static string GetAddress()
        {
            return requestUrl;
        }



    }
}
