using PiraeusClientModule.Clients.Http;
using PiraeusClientModule.Clients.ModBus;
using System.Threading.Tasks;


//DOTNET Core app in Linux container 


namespace PiraeusClientModule
{
    class Program
    {
        //static IChannel channel;
        //static string PSK_IDENTITY = "Realift";
        //static byte[] PSK = Encoding.UTF8.GetBytes("The quick brown fox");
        //static int TCP_BLOCK_SIZE = 1024;
        //static int TCP_MAX_BUFFER_SUZE = 10240;
        //static int MQTT_PORT = 8883;
        //static string PIRAEUS_HOSTNAME = "piraeus.uswest.cloudapp.net";
        //static CancellationTokenSource cts;
        //static PiraeusMqttClient client;
        //static string MODBUS_REQUEST_RESOURCE = "http://www.skunklab.io/realift/modbus/request/rtu1";
        //static string MODBUS_RESPONSE_RESOURCE = "http://www.skunklab.io/realift/modbus/response/rtu1";

        private static ModBusClient client;


        static void Main(string[] args)
        {
            ModBusClientConfig config = new ModBusClientConfig();
            client = new ModBusClient(config);
            client.OnCloudMessage += Client_OnCloudMessage;
            Task task = client.ConnectAsync();
            Task.WhenAll(task);


        }

        /// <summary>
        /// Receives a message from ClearSCADA via V-RTU and Piraeus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Client_OnCloudMessage(object sender, ModBusMessageEventArgs e)
        {
            //send this message to the RTU via ModBus Protocol Adapter over HTTP - POST using application/octet-stream content type.
            GenericHttpClient httpClient = new GenericHttpClient("URL of MOdBus Protocol Adapter to POST message");
            Task<byte[]> task = httpClient.SendAsync(e.Payload);
            Task.WaitAll(task);
            byte[] output = task.Result;

            if(output != null)   //return the output of the RTU to the cloud
            {
                Task sendTask = client.SendAsync(output);
                Task.WhenAll(sendTask);
            }
        }
    }
}
