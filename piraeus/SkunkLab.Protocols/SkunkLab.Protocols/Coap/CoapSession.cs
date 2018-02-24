//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Timers;

//namespace SkunkLab.Protocols.Coap
//{
//    public class CoapSession
//    {   
//        public CoapSession(CoapConfig config)
//        {
//            this.ackTimers = new Dictionary<ushort, Timer>();
//            this.lifetimeTimers = new Dictionary<ushort, Timer>();
//            this.idQuarantine = new HashSet<ushort>();
//        }

//        private HashSet<ushort> idQuarantine;
//        private Dictionary<ushort, Timer> ackTimers;
//        private Dictionary<ushort, Timer> lifetimeTimers;


//        public async Task ReceiveAsync(CoapMessage message)
//        {
//            //determine whether Request or Response

//        }

//        public async Task SendAsync(CoapMessage message)
//        {
//            //determine expectation of response, if any
//        }

        



        

//    }
//}
