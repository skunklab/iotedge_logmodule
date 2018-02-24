using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkunkLab.Channels;

namespace SkunkLab.Core
{
    public class CoapProtocolAdapter : ProtocolAdapter
    {
        public CoapProtocolAdapter(IChannel channel)
        {
            Channel = channel;            
        }


        public override IChannel Channel { get; set; }

        public override event ProtocolAdapterErrorHandler OnError;
        public override event ProtocolAdapterCloseHandler OnClose;




        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }
    }
}
