using Newtonsoft.Json;
using SkunkLab.Edge.Gateway;
using SkunkLab.Edge.Gateway.Mqtt;
using SkunkLab.Security.Tokens;
using SkunkLab.VirtualRtu.Adapters;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HackHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            //string jstring = "{\"claims\":[{\"claimType\":\"http://www.schneider-electric.com/virtualrtu/name\",\"value\":\"rtu1\"},{\"claimType\":\"http://www.schneider-electric.com/virtualrtu/unitid\",\"value\":\"1\"},{\"claimType\":\"http://www.schneider-electric.com/virtualrtu/role\",\"value\":\"gatewaydevice\"},{\"claimType\":\"http://www.schneider-electric.com/virtualrtu/fieldgateway\",\"value\":\"fieldgateway1\"}]}";
            //Claimset cs1 = JsonConvert.DeserializeObject<Claimset>(jstring);

            //RtuMap rmap1 = new RtuMap();
            //rmap1.AddResource(3, "http://www.rtu.com/rtu1-in", "http://www.rtu.com/rtu1-out");
            //string s = JsonConvert.SerializeObject(rmap1);

           



            //byte[] psk = new byte[2] { 1, 2 };
            //Claimset cs = new Claimset();
            //cs.AddClaim("type1", "value1");

            //RtuMap rmap = new RtuMap();
            //rmap.AddResource(4, "in", "out");
            
            //EdgeConfig config = new EdgeConfig("myhost", 8883, "myident", psk, "key", 8, "issuer", "aud", 101, 1001, 90, cs, rmap, "moduleCS");
           

            //string js = JsonConvert.SerializeObject(config);




            EdgeTwin edgeTwin = new EdgeTwin();
            Task task = edgeTwin.ConnectAsync();
            Task.WaitAll(task);

            //RtuMap map = new RtuMap();
            //map.AddResource(11, "http://www.rtu.com/rtu1-in", "http://www.rtu.com/rtu1-out");

            //string json = JsonConvert.SerializeObject(map);

        }


        private async Task ConnectTwin()
        {
            EdgeTwin edgeTwin = new EdgeTwin();
            EdgeConfig config = await edgeTwin.ConnectAsync();
            if(config != null)
            {
                string jsonString = JsonConvert.SerializeObject(config);
                //(1) write to file on container
                //(2) close connection and re-open using the new config
                //(3) report back to the twin
                await edgeTwin.ReportAsync(config);            
            }
        }
    }
}
