using Newtonsoft.Json;
using SkunkLab.Edge.Gateway.Mqtt;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkunkLab.Edge.Gateway
{
    class Program
    {
        private static EdgeConfig config;
        private static MqttEdgeClient client;
        private static int delaySeconds;
        private static CancellationTokenSource source;
        private static CancellationTokenSource localProcess;
        private static CancellationToken ctoken;
        private static ManualResetEventSlim done;

        static void Main(string[] args)
        {
            delaySeconds = 20;
            config = new Mqtt.EdgeConfig();
            localProcess = new CancellationTokenSource();
            ctoken = localProcess.Token;
            ctoken.Register(Restart);
            Task task = StartAsync(ctoken);
            Task.WhenAll(task);

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("TCP-UDP Gateway is ready...");
            done.Wait();

            Console.WriteLine("TCP UDP Gateway is exiting.");


        }


        static async Task StartAsync(CancellationToken token)
        {
            bool configured = await LoadConfigAsync();            

            try
            {
                source = new CancellationTokenSource();
                client = new MqttEdgeClient(config, source.Token);
                await client.InitializeAsync();
            }
            catch(Exception ex)
            {               
                source.Cancel();
                await Task.Delay(delaySeconds);
                delaySeconds = delaySeconds * 2;
                await Task.Delay(delaySeconds);
                if(delaySeconds > 120)
                {
                    delaySeconds = 20;
                }

                client = null;
            }
        }

        static void Restart()
        {

        }

        static async Task<bool> LoadConfigAsync()
        {
            try
            {
                EdgeTwin twin = new EdgeTwin();
                EdgeConfig edgeConfig = await twin.ConnectAsync();
                if (config != null)
                {
                    string json = JsonConvert.SerializeObject(edgeConfig);
                    await File.WriteAllBytesAsync("./edgeconfig.json", Encoding.UTF8.GetBytes(json));
                }
                else
                {
                    byte[] jsonBytes = File.ReadAllBytes("./edgeconfig.json");
                    if (jsonBytes != null)
                    {
                        string json = Encoding.UTF8.GetString(jsonBytes);
                        edgeConfig = JsonConvert.DeserializeObject<EdgeConfig>(json);
                    }
                    else
                    {
                        return false;
                    }
                }

                return config != null;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Edge configuration could not be obtained.");
                Trace.TraceError("Edge config fault with '{0}'", ex.Message);
                return false;
            }
        }

        private static async Task StopAsync()
        {
                await Task.CompletedTask;
        }
    }
}
