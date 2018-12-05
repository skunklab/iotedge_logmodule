using Microsoft.WindowsAzure.Storage.Table;

namespace FunctionApp1
{
    public class MyTableEntity : TableEntity
    {
        public MyTableEntity()
        {

        }

        public string Foo { get; set; }
    }
}
