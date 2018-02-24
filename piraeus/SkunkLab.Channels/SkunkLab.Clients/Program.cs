using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Clients
{
    class Program
    {
        private static Client client;
        private static Server server;
        static void Main(string[] args)
        {
            Console.Write("Client or Server (C/S) ? ");
            
            if(Console.ReadLine().ToLower() == "c")
            {
                client = new Client();
                Task task = client.OpenAsync();
                Task.WhenAll(task);

                Console.WriteLine("Enter message to send ? ");
                string msg = Console.ReadLine();
                Task t = client.SendAsync(Encoding.UTF8.GetBytes(msg));
                Task.WhenAll(t);
            }
            else
            {
                server = new Server(IPAddress.Parse("127.0.0.1"), 12345);
                Task task = server.OpenAsync();
                Task.WhenAll(task);
                Console.WriteLine("Enter message to send ? ");
                string msg = Console.ReadLine();
                Task t = server.SendAsync(Encoding.UTF8.GetBytes(msg));
                Task.WhenAll(t);


            }

            

        }
    }
}
