using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piraeus.Grains.Notifications
{
    public static class RangeIncrementerExtension
    {
        public static int RangeIncrement(this int value, int inclusiveMinimum, int inclusiveMaximum)
        {
            value++;

            if (value < inclusiveMinimum || value > inclusiveMaximum)
            {
                value = inclusiveMinimum;
            }

            return value;
        }

    }
}
