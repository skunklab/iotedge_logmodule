using System.Text;
using Newtonsoft.Json;

namespace Piraeus.Module
{
    public abstract class RestRequestBase
    {
        public abstract T Get<T>();

        public abstract U Post<T, U>(T body)
            where T : class, new();
        public abstract void Post<T>(T body) where T : class;

        public abstract void Post();

        public abstract T Post<T>();
        public abstract void Delete();
        public abstract void Put<T>(T body) where T : class;


        internal byte[] SerializeBody<T>(T body) where T : class
        {
            byte[] byteArray = null;

            string jsonString = JsonConvert.SerializeObject(body);
            byteArray = Encoding.UTF8.GetBytes(jsonString);

            return byteArray;
        }



    }
}
