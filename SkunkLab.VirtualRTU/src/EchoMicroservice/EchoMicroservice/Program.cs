using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EchoMicroservice
{
    public class Program
    {
        //private static string ip;

        public static void Main(string[] args)
        {
            //ip = GetAddress();
            //url = "http://0.0.0.0:8889/";
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                    options.Limits.MaxRequestBodySize = 10 * 1024;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.ListenAnyIP(8889);

                });


        public static string GetAddress()
        {
            string requestUrl = null;
            IPHostEntry entry = Dns.GetHostEntry("echomodule");
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

            return requestUrl;
        }

    }
}
