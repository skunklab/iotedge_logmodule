

namespace SkunkLab.Protocols.Coap
{
    using System;

    [Serializable]
    public class CoapResetArgs : EventArgs
    {
        public CoapResetArgs(ushort messageId, string internalMessageId, CodeType code)
        {
            this.MessageId = messageId;
            this.InternalMessageId = internalMessageId;
            this.Code = code;
        }

        public string InternalMessageId { get; internal set; }
        public ushort MessageId { get; internal set; }
        public CodeType Code { get; internal set; }
    }
}
