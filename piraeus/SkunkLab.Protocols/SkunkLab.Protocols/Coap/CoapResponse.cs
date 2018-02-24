
namespace SkunkLab.Protocols.Coap
{
    using System;
    using System.Linq;
    
    public sealed class CoapResponse : CoapMessage
    {
        public CoapResponse()
            : base()
        {
        }

        public CoapResponse(ushort messageId, ResponseMessageType type, ResponseCodeType code)
            : this(messageId, type, code, null, null, null)
        {
        }

        public CoapResponse(ushort messageId, ResponseMessageType type, ResponseCodeType code, byte[] token)
            : this(messageId, type, code, token, null, null)
        {
        }

        public CoapResponse(ushort messageId, ResponseMessageType type, ResponseCodeType code, byte[] token, MediaType? contentType, byte[] payload)
        {
            this.MessageId = messageId;
            this.ResponseType = type;
            this.ResponseCode = code;
            this.Code = (CodeType)code;
           

            if (token != null)
            {
                this.Token = token;
            }

            if(contentType.HasValue)
            {
                this.ContentType = contentType;
            }

            this.Payload = payload;
            this._options = new CoapOptionCollection();
        }


        public ResponseMessageType ResponseType { get; set; }


        public ResponseCodeType ResponseCode { get; set; }
        
        public bool Error { get; internal set; }
                      

        public override byte[] Encode()
        {
            LoadOptions();
            int length = 0;

            byte[] header = new byte[4 + this.TokenLength];

            int index = 0;            

            header[index++] = (byte)((byte)(0x01 << 0x06) | (byte)(Convert.ToByte((int)ResponseType) << 0x04) | (byte)(this.TokenLength));

            int code = (int)this.Code;
            header[index++] = code < 10 ? (byte)code : (byte)((byte)(Convert.ToByte(Convert.ToString((int)this.ResponseCode).Substring(0, 1)) << 0x05) |
                                                              (byte)(Convert.ToByte(Convert.ToString((int)this.ResponseCode).Substring(1, 2))));

            //header[index++] = (byte)((byte)(Convert.ToByte(Convert.ToString((int)this.ResponseCode).Substring(0, 1)) << 0x05) | 
            //                  (byte)(Convert.ToByte(Convert.ToString((int)this.ResponseCode).Substring(1, 2))));
            header[index++] = (byte)((this.MessageId >> 8) & 0x00FF); //MSB
            header[index++] = (byte)(this.MessageId & 0x00FF); //LSB

            if (this.TokenLength > 0)
            {
                Buffer.BlockCopy(this.Token, 0, header, 4, this.TokenLength);
            }
           
            length += header.Length;

            byte[] options = null;

            if (this.Options.Count > 0)
            {                
                OptionBuilder builder = new OptionBuilder(this.Options.ToArray());
                options = builder.Encode();
                length += options.Length;
            }

            byte[] buffer = null;

            if(this.Payload != null)
            {
                length += this.Payload.Length + 1;
                buffer = new byte[length];
                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                if(options != null)
                {
                    Buffer.BlockCopy(options, 0, buffer, header.Length, options.Length);
                    Buffer.BlockCopy(new byte[] { 0xFF }, 0, buffer, header.Length + options.Length, 1);
                    Buffer.BlockCopy(this.Payload, 0, buffer, header.Length + options.Length + 1, this.Payload.Length);
                }
                else
                {
                    Buffer.BlockCopy(new byte[] { 0xFF }, 0, buffer, header.Length, 1);
                    Buffer.BlockCopy(this.Payload, 0, buffer, header.Length + 1, this.Payload.Length);
                }

                
            }
            else
            {
                buffer = new byte[length];
                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                if (options != null)
                {
                    Buffer.BlockCopy(options, 0, buffer, header.Length, options.Length);
                }
            }

            return buffer;
        }

        public override void Decode(byte[] message)
        {
            //CoapResponse message = new CoapResponse();

            int index = 0;
            byte header = message[index++];
            if (header >> 0x06 != 1)
            {
                throw new CoapVersionMismatchException("Coap Version 1 is only supported version for Coap response.");
            }

            this.ResponseType = (ResponseMessageType)Convert.ToInt32((header >> 0x04) & 0x03);

            this.TokenLength = Convert.ToByte(header & 0x0F);

            byte code = message[index++];
            this.ResponseCode = (ResponseCodeType)(((code >> 0x05) * 100) + (code & 0x1F));

            this.MessageId = (ushort)(message[index++] << 0x08 | message[index++]);
            byte[] tokenBytes = new byte[this.TokenLength];
            Buffer.BlockCopy(message, index, tokenBytes, 0, this.TokenLength);
            this.Token = tokenBytes;

            //get the options
            index += this.TokenLength;
            int previous = 0;
            int delta = 0;
            bool marker = ((message[index] & 0xFF) == 0xFF);

            while (!marker)
            {
                delta = (message[index] >> 0x04);
                CoapOption CoapOption = CoapOption.Decode(message, index, previous, out index);
                this.Options.Add(CoapOption);
                previous += delta;
                marker = ((message[index] & 0xFF) == 0xFF);
            }

            if (marker) //grab the payload
            {
                index++;
                this.Payload = new byte[message.Length - index];
                Buffer.BlockCopy(message, index, this.Payload, 0, this.Payload.Length);
            }

            this.Error = !this.Options.ContainsContentFormat();

            ReadOptions(this);
        }

    }
}
