using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SkunkLab.Diagnostics.Logging
{
    public class Log
    {
        
        public static async Task LogErrorAsync(string msg)
        {
            await LogErrorAsync(msg, null);
        }
        public static async Task LogErrorAsync(string msg, params object[] args)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Trace.TraceError(GetLogString("ERROR", msg, null, args));
            });

            await Task.WhenAll(task);
        }

        public static async Task LogWarningAsync(string msg)
        {
            await LogWarningAsync(msg, null);
        }

        public static async Task LogWarningAsync(string msg, params object[] args)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Trace.TraceWarning(GetLogString("WARNING", msg, null, args));
            });

            await Task.WhenAll(task);
        }

        public static async Task LogInfoAsync(string msg)
        {
            await LogInfoAsync(msg, null);
        }

        public static async Task LogInfoAsync(string msg, params object[] args)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Trace.TraceInformation(GetLogString("INFORMATION", msg, null, args));
            });

            await Task.WhenAll(task);
        }
        public static async Task LogAsync(string msg)
        {
            await LogAsync(msg, null);
        }
        public static async Task LogAsync(string msg, params object[] args)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Trace.WriteLine(GetLogString("VERBOSE", msg, null, args));
            });

            await Task.WhenAll(task);
        }

        private static string GetLogString(string logType, string msg, string category = null, params object[] args)
        {
            string time = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.ffff");

            if (string.IsNullOrEmpty(category))
            {
                return String.Format("{0}{1}{2}", time.PadRight(25), logType.PadRight(12), GetMessageString(msg, args));
            }
            else
            {
                return String.Format("{0}{1}{2}{3}", time.PadRight(25), logType.PadRight(12), category.PadRight(20), GetMessageString(msg, args));
            }
        }

        private static string GetMessageString(string msg, params object[] args)
        {
            if (args == null)
            {
                return msg;
            }
            else
            {
                return String.Format(msg, args);
            }
        }
    }
}
