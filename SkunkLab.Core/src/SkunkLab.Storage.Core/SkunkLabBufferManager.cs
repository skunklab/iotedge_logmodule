using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace SkunkLab.Storage
{
    public class SkunkLabBufferManager : IBufferManager
    {
        public SkunkLabBufferManager(BufferManager manager, int defaultBufferSize)
        {
            this.Manager = manager;
            this.defaultBufferSize = defaultBufferSize;
        }

        private int defaultBufferSize = 0;
        public BufferManager Manager { get; internal set; }

        public int GetDefaultBufferSize()
        {
            return this.defaultBufferSize;
        }

        public void ReturnBuffer(byte[] buffer)
        {
            this.Manager.ReturnBuffer(buffer);
        }

        public byte[] TakeBuffer(int bufferSize)
        {
            return this.Manager.TakeBuffer(bufferSize);
        }
    }
}
