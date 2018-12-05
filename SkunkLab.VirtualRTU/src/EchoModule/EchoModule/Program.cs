using Microsoft.Azure.Devices.Client;
using SkunkLab.VirtualRtu.ModBus;
//using SkunkLab.Edge.Gateway.Modules;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EchoModule
{
    /// <summary>
    /// This module is a test, which receives on one route and echos the result on another.
    /// </summary>
    class Program
    {
        //private static ModuleClient client;
        //private static EdgeModuleClient client;
        private static ManualResetEventSlim done;
        private static ModuleClient moduleClient;
        private static int index;

        static void Main(string[] args)
        {
            Console.WriteLine("Echo Module");
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //need to block
            done = new ManualResetEventSlim(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            try
            {
                Task t = RunAsync();
                Task.WaitAll(t);
            }
            catch(AggregateException ae)
            {
                Console.WriteLine("AggregateException thrown for RunAsync...forcing restart");
                Console.WriteLine(ae.Flatten().InnerException.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception thrown for RunAsync...forcing restart");
                Console.WriteLine(ex.Message);
                done.Set();
            }

            

            Console.WriteLine("Echo Module is ready and blocking.");
            done.Wait();

            Console.WriteLine("Echo Module terminated");
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();

            Console.WriteLine("********** Unobserved Exception Block **********");
            Console.WriteLine("Error = '{0}'", e.Exception.Message);

            Exception inner = e.Exception.InnerException;
            int indexer = 0;
            while (inner != null)
            {
                indexer++;
                Console.WriteLine("Inner index {0} '{1}'", indexer, inner.Message);
                if (String.IsNullOrEmpty(inner.Message))
                {
                    Console.WriteLine("-------------- Start Stack Trace {0} ---------------", indexer);
                    Console.WriteLine(inner.StackTrace);
                    Console.WriteLine("-------------- End Stack Trace {0} ---------------", indexer);
                }
                inner = inner.InnerException;
            }


            Console.WriteLine("********** End Unobserved Exception Block **********");
            Console.WriteLine("<----------> Force Start <---------->");
            done.Set();
        }

        private static async Task RunAsync()
        {
            Console.WriteLine("Starting...");

            try
            {
                moduleClient = await ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt).ConfigureAwait(false);
                var context = moduleClient;
                Console.WriteLine("Opening module client explicitly");
                await moduleClient.OpenAsync().ConfigureAwait(false);
                Console.WriteLine("Module client open");

                Console.WriteLine("Setting input message handler");
                await moduleClient.SetInputMessageHandlerAsync("echoInput", InputHandler, context).ConfigureAwait(false);
                Console.WriteLine("Input message handler set.");
            }
            catch(AggregateException ae)
            {
                Console.WriteLine("AE Exception in Run - {0}", ae.Flatten().InnerException.Message);
                if(ae.Flatten().InnerException.InnerException != null)
                {
                    Console.WriteLine("AE Exception 2 in Run - {0}", ae.Flatten().InnerException.InnerException.Message);
                }
                done.Set();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exceptio in  Run - {0}", ex.Message);
                if(ex.InnerException != null)
                {
                    Console.WriteLine("Exception 2 in Run - {0}", ex.InnerException.Message);
                }
                done.Set();
            }
        }

        private static async Task<MessageResponse> InputHandler(Message message, object context)
        {
            try
            {
                byte[] headerBuffer = new byte[7];
                byte[] msg = message.GetBytes();
                Buffer.BlockCopy(msg, 0, headerBuffer, 0, headerBuffer.Length);
                MbapHeader header = MbapHeader.Decode(headerBuffer);
                index++;
                Console.WriteLine("{0} Message {1} received Transaction ID = {2}", DateTime.Now.ToString("hh:MM:ss.ffff"), index, header.TransactionId);
                ModuleClient mclient = (ModuleClient)context;
                await mclient.CompleteAsync(message).ConfigureAwait(false);
                await mclient.SendEventAsync("echoOutput", new Message(msg)).ConfigureAwait(false);
                Console.WriteLine("{0} Message {1} echo'd", DateTime.Now.ToString("hh:MM:ss.ffff"), index);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("Fault on echo module reccieve with AG exception.");
                Console.WriteLine("AE Exception in Receive - {0}", ae.Flatten().InnerException.Message);
                if (ae.Flatten().InnerException.InnerException != null)
                {
                    Console.WriteLine("AE Exception 2 in Receive - {0}", ae.Flatten().InnerException.InnerException.Message);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fault on echo module reccieve with exception.");
                Console.WriteLine("Exception in  Receive - {0}", ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Exception 2 in Receive - {0}", ex.InnerException.Message);
                }
            }

            return MessageResponse.Completed;
        }


    }
}
