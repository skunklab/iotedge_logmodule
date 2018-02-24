using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public static class TraceManager
    {
        public static void WriteInfo(int code, string message)
        {
            string traceMessage = Properties.Resources.ResourceManager.GetString(String.Format("SLTI{0}", code));
            Trace.TraceInformation(String.Format(traceMessage, message));
        }
        public static void WriteWarning(int code, string message)
        {
            string traceMessage = Properties.Resources.ResourceManager.GetString(String.Format("SLTW{0}", code));
            Trace.TraceWarning(String.Format(traceMessage, message));
        }

        public static void WriteError(int code, string message)
        {
            string traceMessage = Properties.Resources.ResourceManager.GetString(String.Format("SLTE{0}", code));
            Trace.TraceError(String.Format(traceMessage, message));
        }
    }
}
