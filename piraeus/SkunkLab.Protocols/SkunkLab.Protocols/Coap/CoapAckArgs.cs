


namespace SkunkLab.Protocols.Coap
{
    using System;

    [Serializable]
    public class CoapAckArgs : EventArgs
    {
        public CoapAckArgs(ushort messageId, string internalMessageId, byte[] token, CodeType code, string faultMessage)
        {
            this.MessageId = messageId;
            this.InternalMessageId = internalMessageId;
            this.Token = token;
            this.Code = code;
            this.FaultMessage = faultMessage;
        }

        public CoapAckArgs(ushort messageId, string internalMessageId, byte[] token, CodeType code, string contentType, byte[] responseMessage)
        {
            this.MessageId = messageId;
            this.InternalMessageId = internalMessageId;
            this.Token = token;
            this.Code = code;
            this.ContentType = contentType;
            this.ResponseMessage = responseMessage;
        }

        public ushort MessageId { get; internal set; }
        public byte[] Token { get; internal set; }
        public CodeType Code { get; internal set; }
        public string FaultMessage { get; internal set; }
        public string ContentType { get; internal set; }
        public byte[] ResponseMessage { get; internal set; }
        public string InternalMessageId { get; internal set; }

    }
}
