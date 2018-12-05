using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EchoMicroservice
{
    public class MyLittleHelper
    {
        static MyLittleHelper()
        {
            queue = new Queue<byte[]>();
            client = new HttpClient();
            IPHostEntry entry = Dns.GetHostEntry("fieldgateway");
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
                requestUrl = String.Format("http://{0}:8888/api/rtuoutput", ipAddressString);
                Console.WriteLine("REQUEST URL = '{0}'", requestUrl);
            }
            else
            {
                Console.WriteLine("NO IP ADDRESS FOUND");
            }
        }

        private static HttpClient client;
        private static Queue<byte[]> queue;
        private static string requestUrl;


        public static Queue<byte[]> Queue
        {
            get { return queue; }
            
        }

        public static async Task ForwardAsync(byte[] payload)
        {
            try
            {
                HttpContent content = new ByteArrayContent(payload);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Headers.ContentLength = payload.Length;
                HttpResponseMessage response = await client.PostAsync(requestUrl, content);

                Console.WriteLine("Messag echo'd with status code '{0}'", response.StatusCode);
            }
            catch (WebException we)
            {
                Console.WriteLine("Web Exception forwarding '{0}'", we.Message);
                if (we.InnerException != null)
                {
                    Console.WriteLine("Web Inner Exception '{0}'", we.InnerException.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception forwarding '{0}'", ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception '{0}'", ex.InnerException.Message);
                }

            }
        }

        public static async Task EchoAsync(byte[] payload)
        {
            try
            {
                Console.WriteLine("{0} - Start echo", DateTime.Now.ToString("hh:MM:ss.ffff"));
                HttpContent content = new ByteArrayContent(payload);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Headers.ContentLength = payload.Length;
                HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                Console.WriteLine("{0} - End echo", DateTime.Now.ToString("hh:MM:ss.ffff"));
            }
            catch (WebException we)
            {
                Console.WriteLine("{0} - Echo fault", DateTime.Now.ToString("hh:MM:ss.ffff"));
                Console.WriteLine("Web Exception forwarding '{0}'", we.Message);
                if (we.InnerException != null)
                {
                    Console.WriteLine("Web Inner Exception '{0}'", we.InnerException.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception forwarding '{0}'", ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception '{0}'", ex.InnerException.Message);
                }

            }
        }
    }
}
