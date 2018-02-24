

namespace SkunkLab.Protocols.Coap
{
    using System;
    public class CoapTimerArgs : EventArgs
    {
        public CoapTimerArgs(int retryAttempt, DateTime startTime, CoapMessage message, string internalMessageId)
        {
            this.retryAttempt = retryAttempt;
            this.Message = message;
            this.startTime = startTime;
            this.InternalMessageId = internalMessageId;
        }

        private DateTime startTime;
        private int retryAttempt;
        public CoapMessage Message { get; internal set; }

        public ushort MessageId
        {
            get { return this.Message.MessageId; }
        }

        public byte[] Token
        {
            get { return this.Message.Token; }
        }

        public string InternalMessageId { get; set; }

        public bool MaxTransmitSpanExceeded
        {
            get
            {
                TimeSpan diff = DateTime.Now.Subtract(this.startTime);
                return (diff.Milliseconds >= CoapConstants.Timeouts.MaxTransmitSpan.Milliseconds);
            }
        }

        public bool RetriesExceeded
        {
            get
            {
                return retryAttempt >= CoapConstants.Timeouts.MaxRetransmit;
            }
        }
    }
}
