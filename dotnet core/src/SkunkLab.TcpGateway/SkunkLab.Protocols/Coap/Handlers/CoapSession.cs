using System;
using System.Collections.Generic;
using System.Timers;
using SkunkLab.Security.Tokens;

namespace SkunkLab.Protocols.Coap.Handlers
{
    public delegate void EventHandler<CoapMessageEventArgs>(object sender, CoapMessageEventArgs args);
    public delegate CoapMessage RespondingEventHandler(object sender, CoapMessageEventArgs args);
    public class CoapSession : IDisposable
    {
        public CoapSession(CoapConfig config)
        {
            Config = config;
            
            CoapReceiver = new Receiver(config.ExchangeLifetime.TotalMilliseconds);
            CoapSender = new Transmitter(config.ExchangeLifetime.TotalMilliseconds, config.MaxTransmitSpan.TotalMilliseconds, config.MaxRetransmit);
            CoapSender.OnRetry += Transmit_OnRetry;

            if(config.KeepAlive.HasValue)
            {
                keepaliveTimestamp = DateTime.UtcNow.AddSeconds(config.KeepAlive.Value);
                keepaliveTimer = new Timer(config.KeepAlive.Value * 1000);
                keepaliveTimer.Elapsed += KeepaliveTimer_Elapsed;
                keepaliveTimer.Start();
            }
        }
        

        public event EventHandler<CoapMessageEventArgs> OnRetry;
        public event EventHandler<CoapMessageEventArgs> OnKeepAlive;
        private bool disposedValue;
        private DateTime keepaliveTimestamp;
        private Timer keepaliveTimer;
        private string bootstrapToken;
        private SecurityTokenType bootstrapTokenType;


        public bool HasBootstrapToken { get; internal set; }

        public string Identity { get; set; }

        public List<KeyValuePair<string,string>> Indexes { get; set; }

        public bool IsAuthenticated { get; set; }
       

        public Transmitter CoapSender { get; internal set; }

        public Receiver CoapReceiver { get; internal set; }
                
        

        public CoapConfig Config { get; internal set; }

        
        public bool Authenticate(string tokenType, string token)
        {
            if (HasBootstrapToken)
            {
                IsAuthenticated = Config.Authenticator.Authenticate(bootstrapTokenType, bootstrapToken);
            }
            else
            {
                SecurityTokenType tt = (SecurityTokenType)Enum.Parse(typeof(SecurityTokenType), tokenType, true);
                bootstrapTokenType = tt;
                bootstrapToken = token;
                IsAuthenticated = Config.Authenticator.Authenticate(tt, token);
                HasBootstrapToken = true;
            }

            return IsAuthenticated;
        }


        public bool IsNoResponse(NoResponseType? messageNrt, NoResponseType result)
        {
            
            if(!messageNrt.HasValue)
            {
                return false;
            }

            return messageNrt.Value.HasFlag(result);
        }

        public bool CanObserve()
        {
            return Config.ConfigOptions.HasFlag(CoapConfigOptions.Observe);
        }

        public void UpdateKeepAliveTimestamp()
        {
            keepaliveTimestamp = DateTime.UtcNow.AddMilliseconds(Config.KeepAlive.Value);
        }

        private void KeepaliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (keepaliveTimestamp <= DateTime.UtcNow)
            {
                //signal a ping
                CoapToken token = CoapToken.Create();
                ushort id = CoapSender.NewId(token.TokenBytes);
                CoapRequest ping = new CoapRequest()
                {
                    MessageId = id,
                    Token = token.TokenBytes,
                    Code = CodeType.EmptyMessage,
                    MessageType = CoapMessageType.Confirmable
                };

                OnKeepAlive?.Invoke(this, new CoapMessageEventArgs(ping));
            }
        }

        private void Transmit_OnRetry(object sender, CoapMessageEventArgs e)
        {
            OnRetry?.Invoke(this, e);
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if(keepaliveTimer != null)
                    {
                        keepaliveTimer.Stop();
                        keepaliveTimer.Dispose();
                    }

                    CoapSender.Dispose();
                    CoapReceiver.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
