using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Protocols.Coap
{
    public class IdManager
    {
        public IdManager()
        {
            container = new HashSet<ushort>();
        }

        private HashSet<ushort> container;
        private ushort currentId;
        private ushort NewId()
        {
            currentId++;

            while (container.Contains(currentId))
            {
                currentId = currentId == ushort.MaxValue ? (ushort)1 : currentId;
            }

            container.Add(currentId);

            return currentId;
        }

        private void Remove(ushort id)
        {
            container.Remove(id);
        }
    }
}
