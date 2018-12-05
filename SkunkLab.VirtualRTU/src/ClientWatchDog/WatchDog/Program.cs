using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WatchDog
{
    class Program
    {
        private static string parameters;
        private static Process process;
        private static int id;
        static void Main(string[] args)
        {
            SetArgString(args);

            while(true)
            {
                //Console.WriteLine("Start loop");

                if(process == null)
                {
                    Console.WriteLine("Process is null");
                    Task t0 = StartClient();
                    Task.WhenAll(t0);
                }  
                else
                {
                    if (id > 0)
                    {

                        Process p = Process.GetProcessById(id);
                        //Console.WriteLine("Process found is null {0}", p == null);
                        
                        if (p == null)
                        {
                            Task t1 = StartClient();
                            Task.WhenAll(t1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Process running...");
                    }
                }
                Thread.Sleep(10000);
            }
        }

        private static void SetArgString(string[] args)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("..\\Test\\TestScadaClient.dll ");
            foreach(string arg in args)
            {
                builder.Append(arg);
                builder.Append(" ");
            }

            parameters = builder.ToString();
        }

        private static async Task StartClient()
        {
            Console.WriteLine("Starting client");
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "dotnet";
            info.Arguments = parameters;
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.UseShellExecute = true;
            process = Process.Start(info);
            process.EnableRaisingEvents = true;
            id = process.Id;
            process.Exited += Process_Exited;
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("Process exited waiting 30 sec before restart");
            Thread.Sleep(30000);
            Task task = StartClient();
            Task.WhenAll(task); 
        }
    }
}
