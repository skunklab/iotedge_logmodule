using Newtonsoft.Json;
using SkunkLab.Edge.Gateway.Mqtt;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualRtu.Common.Configuration;

namespace SkunkLab.Edge.Gateway
{
    class Program
    {
        private static string cspath = "../config/moduleConnectionString.txt";
        private static string configpath = "../config/config.json";
        private static EdgeConfig config;
        private static IssuedConfig issuedConfig;
        private static MqttEdgeClient client;
        private static int delaySeconds;
        private static CancellationTokenSource source;
        private static CancellationTokenSource localProcess;
        private static CancellationToken ctoken;
        private static ManualResetEventSlim done;
        private static EdgeTwin edgeTwin;
        private static string moduleConnectionString;

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
            try
            {
                await LoadConfigAsync();
                byte[] filecsBytes = File.ReadAllBytes(cspath);
                string moduleConnectionString = Encoding.UTF8.GetString(filecsBytes);
                source = new CancellationTokenSource();
                client = new MqttEdgeClient(issuedConfig, moduleConnectionString, source.Token);
                await client.InitializeAsync();
            }
            catch (FileNotFoundException fnfe)
            {
                Trace.TraceWarning("Fatal fault starting field gateway.");
                Trace.TraceError(fnfe.Message);
            }
            catch (Exception ex)
            {
                await RestartAsync(ex);
            }
        }


       
        static void Restart()
        {
            localProcess = new CancellationTokenSource();
            Task task = RestartAsync(null);
            Task.WhenAll(task);
        }

        static async Task RestartAsync(Exception ex)
        {
            if (ex != null)
            {
                Trace.TraceWarning("Nonfatal exception starting field gateway must restart.");
                Trace.TraceError(ex.Message);
            }

            if (source != null)
            {
                source.Cancel();
            }

            await Task.Delay(delaySeconds);
            Random ran = new Random();
            double factor = 1.0 + ran.NextDouble();
            delaySeconds = delaySeconds + Convert.ToInt32(Convert.ToDouble(delaySeconds) * factor);
            await Task.Delay(delaySeconds);
            if (delaySeconds > 120)
            {
                delaySeconds = 20;
            }

            client = null;

            source = new CancellationTokenSource();
            Task task = StartAsync(source.Token);
            await Task.WhenAll(task);
        }

        static async Task LoadConfigAsync()
        {
            try
            {
                if (!File.Exists(cspath)) //no module connection string
                {
                    throw new FileNotFoundException("Module connection string file not found.");
                }
                else
                {
                    moduleConnectionString = File.ReadAllText(cspath);
                }

                //opening the module twin
                if(edgeTwin != null)
                {
                    await edgeTwin.CloseAsync();
                }

                edgeTwin = new EdgeTwin();               
                edgeTwin.OnConfiguration += EdgeTwin_OnConfiguration; //if the event is raised, then reconfigure
                await edgeTwin.ConnectAsync(moduleConnectionString);
                

                if (File.Exists(configpath)) //existing configuration available...use it
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(configpath);
                    string jsonString = Encoding.UTF8.GetString((fileBytes));
                    issuedConfig = JsonConvert.DeserializeObject<IssuedConfig>(jsonString);                   
                }                
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Edge configuration could not be obtained.");
                Trace.TraceError("Edge config fault with '{0}'", ex.Message);
                throw ex;
            }
        }

        private static void EdgeTwin_OnConfiguration(object sender, ModuleConfigurationEventArgs e)
        {
            try
            {
                Task<IssuedConfig> task = GetConfigurationAsync(e.Config.LUSS, e.Config.ServiceUrl);
                Task.WaitAll(task);

                IssuedConfig config = task.Result;

                if (config != null)
                {
                    issuedConfig = config;
                    string jsonString = JsonConvert.SerializeObject(issuedConfig);
                    File.WriteAllText(configpath, jsonString);
                    //restart module                    
                }
                else
                {
                    if (issuedConfig == null)
                    {
                        //restart module; otherwise it is configured
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Module twin configuration event fault.");
                Trace.TraceError(ex.Message);
            }
        }

        private static async Task StopAsync()
        {
                await Task.CompletedTask;
        }


        private static async Task<IssuedConfig> GetConfigurationAsync(string luss, string url)
        {
            try
            {
                ConfigurationModel model = new ConfigurationModel(luss, url);
                string jsonString = await model.GetConfiguration();
                if (!string.IsNullOrEmpty(jsonString))
                {
                    return JsonConvert.DeserializeObject<IssuedConfig>(jsonString);
                }
                else
                {
                    return null;
                }
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Configuration service fault.");
                Trace.TraceError(ex.Message);
            }

            return null;
        }
    }
}
