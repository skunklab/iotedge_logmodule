using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkunkLab.Edge
{
    public class ConfigurationModel
    {
        public ConfigurationModel(string luss, string url)
        {
            requestUrl = String.Format("{0}&luss={1}", url, luss);
        }

        private string requestUrl;

        public async Task<string> GetConfiguration()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage message = await client.GetAsync(requestUrl);
            if(message.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await message.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }
    }
}
