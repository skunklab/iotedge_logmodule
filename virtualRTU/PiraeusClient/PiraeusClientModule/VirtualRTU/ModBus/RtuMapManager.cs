using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace VirtualRTU.ModBus
{
    public class RtuMapManager
    {

        public RtuMapManager()
        {
            container = new Dictionary<ushort, Tuple<string, string>>();
        }

        private Dictionary<ushort, Tuple<string,string>> container;

        public static RtuMapManager instance;
        public bool IsLoaded { get; internal set; }

        
        public static RtuMapManager Create()
        {
            if(instance == null)
            {
                instance = new RtuMapManager();
            }

            return instance;
        }

        public static RtuMapManager Load(RtuMapItem[] items)
        {
            RtuMapManager map = RtuMapManager.Create();
            if(map.IsLoaded)
            {
                return map;
            }

            foreach(RtuMapItem item in items)
            {
                map.Add(new Tuple<ushort, string, string>(item.UnitId, item.PublishResource, item.SubscribeResource));
            }

            return map;
        }

        public ushort[] GetUnitIds()
        {
            return container.Keys.ToArray();
        }


        private static async Task<string> GetMap(string serviceUrl)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(serviceUrl);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }





        public void Add(Tuple<ushort,string, string> item)
        {

            if(container.ContainsKey(item.Item1))
            {
                container.Remove(item.Item1);
            }

            container.Add(item.Item1, new Tuple<string, string>(item.Item2, item.Item3));
        }

        public string GetPublishResource(ushort unitId)
        {
            if(container.ContainsKey(unitId))
            {
                return container[unitId].Item1;
            }

            return null;
        }

        public string GetSubscribeResource(ushort unitId)
        {
            if (container.ContainsKey(unitId))
            {
                return container[unitId].Item2;
            }

            return null;
        }

    }
}
