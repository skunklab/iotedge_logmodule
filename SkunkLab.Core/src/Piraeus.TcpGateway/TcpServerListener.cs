using Piraeus.Adapters;
using Piraeus.Configuration.Settings;
using Piraeus.Core;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.TcpGateway
{
    public class TcpServerListener
    {
        public TcpServerListener(IPEndPoint localEP, PiraeusConfig config, CancellationToken token)
        {
            serverIP = localEP.Address;
            serverPort = localEP.Port;
            listener = new TcpListener(localEP);
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;

            if (config.Security.Client.TokenType != null && config.Security.Client.SymmetricKey != null)
            {
                SecurityTokenType stt = (SecurityTokenType)System.Enum.Parse(typeof(SecurityTokenType), config.Security.Client.TokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.Security.Client.SymmetricKey, config.Security.Client.Issuer, config.Security.Client.Audience);
                this.authn = bauthn;
            }
        }

        public TcpServerListener(IPAddress address, int port, PiraeusConfig config, CancellationToken token)
        {
            serverIP = address;
            serverPort = port;
            listener = new TcpListener(address, port);
            listener.ExclusiveAddressUse = false;
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;

            if (config.Security.Client.TokenType != null && config.Security.Client.SymmetricKey != null)
            {
                SecurityTokenType stt = (SecurityTokenType)System.Enum.Parse(typeof(SecurityTokenType), config.Security.Client.TokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.Security.Client.SymmetricKey, config.Security.Client.Issuer, config.Security.Client.Audience);
                this.authn = bauthn;
            }
        }

        public event EventHandler<ServerFailedEventArgs> OnError;
        private IPAddress serverIP;
        private int serverPort;
        private TcpListener listener;
        private CancellationToken token;
        private Dictionary<string, ProtocolAdapter> dict;
        private PiraeusConfig config;
        private IAuthenticator authn;

        public async Task StartAsync()
        {
            Trace.TraceInformation("<----- TCP Listener staring on Address {0} and Port {1} ----->", serverIP.ToString(), serverPort);
            listener.ExclusiveAddressUse = false;
            listener.Start();

            Console.WriteLine("Listener started on IP {0} Port {1}", serverIP.ToString(), serverPort);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    client.LingerState = new LingerOption(false, 0);
                    client.NoDelay = true;
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Client.UseOnlyOverlappedIO = true;
                    ManageConnection(client);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new ServerFailedEventArgs("TCP", serverPort));
                    Trace.TraceError("{0} - TCP server listener failed to start '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                }
            }
        }

        public async Task StopAsync()
        {
            Trace.TraceInformation("{0} - <----- TCP Listener stopping on Address {1} and Port {2} ----->", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),serverIP.ToString(), serverPort);
            Console.WriteLine("{0} - <----- TCP Listener stopping on Address {1} and Port {2} ----->", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), serverIP.ToString(), serverPort);
            //dispose all adapters first
            if (dict != null & dict.Count > 0)
            {
                Trace.TraceInformation("'{0}' - Protocols adapters in Listener to remove and dispose.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                Console.WriteLine("'{0}' - Protocols adapters in Listener to remove and dispose.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                var keys = dict.Keys;
                if (keys != null && keys.Count > 0)
                {
                    try
                    {
                        string[] keysArray = keys.ToArray();
                        foreach (var key in keysArray)
                        {
                            if (dict.ContainsKey(key))
                            {
                                ProtocolAdapter adapter = dict[key];
                                dict.Remove(key);
                                try
                                {
                                    adapter.Dispose();
                                    Console.WriteLine("{0} - TCP Listener stopping and dispose Protcol adapter {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), key);

                                }
                                catch (Exception ex)
                                {
                                    Trace.TraceWarning("{0} - Fault dispose protcol adaper while Stopping TCP Listener - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),ex.Message);
                                    Trace.TraceError("{0} - Fault dispose protcol adaper while Stopping TCP Listener stack trace - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.StackTrace);
                                    Console.WriteLine("{0} - Fault dispose protcol adaper while Stopping TCP Listener - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning("{0} - TCP Listener fault attempting to dispose protocol adapters", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                        Trace.TraceError("{0} - TCP Listener fault - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),ex.Message);
                        Trace.TraceError("{0} - TCP Listener fault stack trace- {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.StackTrace);

                    }
                }
            }
            else
            {
                Trace.TraceInformation("{0} - No protocol adapters in Listener dictionary to dispose and remove", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                Console.WriteLine("{0} - No protocol adapters in Listener dictionary to dispose and remove", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
            }

            listener.Stop();

            await Task.CompletedTask;
        }

        private void ManageConnection(TcpClient client)
        {
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, authn, client, token);
            dict.Add(adapter.Channel.Id, adapter);
            adapter.OnError += Adapter_OnError;
            adapter.OnClose += Adapter_OnClose;
            adapter.Init();
            adapter.Channel.OpenAsync().LogExceptions();
            adapter.Channel.ReceiveAsync().LogExceptions();
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs args)
        {
            Trace.TraceWarning("{0} - Protocol adapter on channel {1} closing.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), args.ChannelId);

            try
            {
                if (dict.ContainsKey(args.ChannelId))
                {
                    ProtocolAdapter adapter = dict[args.ChannelId];
                    dict.Remove(args.ChannelId);
                    adapter.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} - Exception disposing adapter Listener_OnClose - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                Trace.TraceWarning("{0} - TCP Listener exception disposing adapter Listener_OnClose", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                Trace.TraceError("{0} - Adapter dispose exception Listener_OnClose - '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),ex.Message);
                Trace.TraceError("{0} - Adapter dispose stack trace Listener_OnClose - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.StackTrace);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs args)
        {
            Trace.TraceError("{0} - Protocol Adapter on channel {1} threw error {2}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), args.ChannelId, args.Error.Message);
            Console.WriteLine("{0} - Adpater exception - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), args.Error.Message);

            Exception inner = args.Error.InnerException;
            while (inner != null)
            {
                Console.WriteLine("{0} - Adapter Exception Inner - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), inner.Message);
                inner = inner.InnerException;
            }

            Trace.WriteLine("------ Stack Trace -------");
            Trace.TraceError(args.Error.StackTrace);
            Trace.WriteLine("------ End Stack Trace -------");

            try
            {
                if (dict.ContainsKey(args.ChannelId))
                {
                    ProtocolAdapter adapter = dict[args.ChannelId];
                    dict.Remove(args.ChannelId);
                    adapter.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} - Exception disposing adapter Listener_OnError - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                Trace.TraceWarning("{0} - TCP Listener exception disposing adapter Listener_OnError", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                Trace.TraceError("{0} - Adapter dispose exception Listener_OnError - '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),ex.Message);
                Trace.TraceError("{0} - Adapter dispose stack trace Listener_OnError - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.StackTrace);
            }
        }
    }
}
