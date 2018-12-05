using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ConsoleApp1
{
    class Program
    {


        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=virtualrtu;AccountKey=QtSJkEFvjMUCowJ9hDMKbpG67jWMxDFLEYQuArnfUqr+lmgstzdhXuNrexUfMp7D6Sr6HJoisDmA/WhmwSjdLA==;EndpointSuffix=core.windows.net";

        static void Main(string[] args)
        {

            LussEntity entity = new LussEntity();
            entity.Luss = "TEST";
            entity.UnitId = 13;
            entity.DeviceId = "device13";
            entity.ModuleId = "fieldgateway1";
            entity.VirtualRtuId = "alberta";
            entity.Created = DateTime.UtcNow;
            entity.Expires = DateTime.UtcNow.AddDays(1);

            Task task = entity.UpdateAsync("rtutokens",connectionString);
            Task.WaitAll(task);

            Console.ReadKey();


            //string containerName = "map";
            //string filename = "rtumap.json";

            //RtuMap map = RtuMap.LoadFromConnectionString(containerName, filename, connectionString);

            //PrintDictionary(map);

            //map.Map.Add(88, new ResourceItem("http://www.foo.com/bar1", "http://www.foo.com/bar2"));
            //Console.WriteLine("------------------------------");
            //PrintDictionary(map);





            //Task t = map.UpdateMapAsync(containerName, filename, connectionString);
            //Task.WaitAll(t);

            //RtuMap map2 = RtuMap.LoadFromConnectionString(containerName, filename, connectionString);

            //Console.WriteLine("------------------------------");
            //PrintDictionary(map2);

            Console.ReadKey();
        }


        static void PrintDictionary(RtuMap map)
        {
            Dictionary<ushort, ResourceItem>.Enumerator en = map.Map.GetEnumerator();
            while(en.MoveNext())
            {
                Console.WriteLine("{0} : {1}", en.Current.Key, en.Current.Value.RtuInputResource);
            }
        }


        
    }
}
