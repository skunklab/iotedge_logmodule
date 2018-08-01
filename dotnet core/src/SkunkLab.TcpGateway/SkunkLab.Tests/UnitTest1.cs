using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SkunkLab.VirtualRtu.Adapters;

namespace SkunkLab.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string rtuSendUri = "http://www.example.org/rtu/send1";
            string rtuReceiveUri = "http://www.example.org/rtu/receive1";
            ushort unitId = 3;
            RtuMap rtuMap = new RtuMap();
            rtuMap.AddResource(unitId, rtuReceiveUri, rtuSendUri);

            string json = JsonConvert.SerializeObject(rtuMap);
            RtuMap expected = JsonConvert.DeserializeObject<RtuMap>(json);
            Assert.IsTrue(rtuReceiveUri == expected.GetResources(unitId).RtuInputResource);
            Assert.IsTrue(rtuSendUri == expected.GetResources(unitId).RtuOutputResource);
        }
    }
}
