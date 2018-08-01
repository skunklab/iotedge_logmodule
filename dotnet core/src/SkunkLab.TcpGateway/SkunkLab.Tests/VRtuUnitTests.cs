using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SkunkLab.VirtualRtu.Adapters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkunkLab.Tests
{
    [TestClass]
    public class VRtuUnitTests
    {
        [TestMethod]
        public void SeriaizeRtuMapTest()
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

        [TestMethod]
        public void DeseriaizeRtuMapTest()
        {
            string jsonString = "{\"map\":{\"3\":{\"Item1\":\"http://www.example.org/rtu/receive1\",\"Item2\":\"http://www.example.org/rtu/send1\"}}}";
            string expectedRtuSendUri = "http://www.example.org/rtu/send1";
            string expectedRtuReceiveUri = "http://www.example.org/rtu/receive1";
            ushort unitId = 3;

            RtuMap map = JsonConvert.DeserializeObject<RtuMap>(jsonString);

            Assert.IsTrue(map.Map.ContainsKey(unitId));
            Assert.AreEqual(expectedRtuReceiveUri, map.Map[unitId].RtuInputResource);
            Assert.AreEqual(expectedRtuSendUri, map.Map[unitId].RtuOutputResource);
        }


        [TestMethod]
        public void AddRtuMapItemTest()
        {
            string expectedRtuSendUri = "http://www.example.org/rtu/send1";
            string expectedRtuReceiveUri = "http://www.example.org/rtu/receive1";
            ushort unitId = 3;
            RtuMap rtuMap = new RtuMap();
            rtuMap.AddResource(unitId, expectedRtuReceiveUri, expectedRtuSendUri);

            Assert.IsTrue(rtuMap.Map.ContainsKey(unitId));
            Assert.IsTrue(expectedRtuReceiveUri == rtuMap.GetResources(unitId).RtuInputResource);
            Assert.IsTrue(expectedRtuSendUri == rtuMap.GetResources(unitId).RtuOutputResource);
        }

        [TestMethod]
        public void CheckForRtuMapItemTest()
        {
            string expectedRtuSendUri = "http://www.example.org/rtu/send1";
            string expectedRtuReceiveUri = "http://www.example.org/rtu/receive1";
            ushort unitId = 3;
            RtuMap rtuMap = new RtuMap();
            rtuMap.AddResource(unitId, expectedRtuReceiveUri, expectedRtuSendUri);

            Assert.IsTrue(rtuMap.Map.ContainsKey(unitId));
        }


        [TestMethod]
        public void RemoveRtuMapItemTest()
        {
            string expectedRtuSendUri = "http://www.example.org/rtu/send1";
            string expectedRtuReceiveUri = "http://www.example.org/rtu/receive1";
            ushort unitId = 3;
            RtuMap rtuMap = new RtuMap();
            rtuMap.AddResource(unitId, expectedRtuReceiveUri, expectedRtuSendUri);
            rtuMap.RemoveResource(unitId);

            Assert.IsTrue(rtuMap.Map.Count == 0);
        }
    }
}
