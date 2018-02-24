

namespace Piraeus.Protocols.Coap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class CoapMessage
    {
        public static CoapMessage DecodeMessage(byte[] message)
        {
            CoapMessage CoapMessage = new CoapMessage();
            CoapMessage.Decode(message);
            CoapMessage.MessageBytes = message;
            return CoapMessage;
        }

        public CoapMessage()
        {
            this._options = new CoapOptionCollection();
            this._locationPath = new List<string>();
            this._locationQuery = new List<string>();
            this._ifMatch = new List<byte[]>();
            this._eTag = new List<byte[]>();
        }

        protected byte version = 1;
        protected byte _tokenLength;
        protected byte[] _token;
        internal CoapOptionCollection _options;
        protected List<byte[]> _ifMatch;
        protected List<byte[]> _eTag;
        protected List<string> _locationPath;
        protected List<string> _locationQuery;
        protected uint _maxAge = 60;

        public virtual byte[] MessageBytes { get; internal set; }

        public virtual bool HasContentFormat { get; internal set; }

        public virtual ushort MessageId { get; set; }
        public virtual Uri ResourceUri { get; set; }

        public virtual List<byte[]> IfMatch
        {
            get { return this._ifMatch; }
        }

        public virtual List<byte[]> ETag
        {
            get { return this._eTag; }
            internal set { this._eTag = value; }
        }

        public virtual bool IfNoneMatch { get; set; }
        
        public virtual List<string> LocationPath
        {
            get { return this._locationPath; }
        }
        
        public virtual MediaType? ContentType { get; set; }

        public virtual uint MaxAge
        {
            get { return this._maxAge; }
            set { this._maxAge = value; }
        }

        public virtual MediaType? Accept { get; set; }

        public virtual List<string> LocationQuery
        {
            get { return this._locationQuery; }
        }
        public virtual string ProxyUri { get; set; }

        public virtual CoapMessageType MessageType { get; set; }

        public virtual CodeType Code { get; set; }

        public virtual string ProxyScheme { get; set; }

        public virtual uint Size1 { get; set; }

        public virtual byte[] Token
        {
            get { return this._token; }
            set
            {
                if (value != null)
                {
                    this.TokenLength = (byte)value.Length;
                    this._token = value;
                }
            }
        }

        public virtual byte[] Payload { get; set; }

        public virtual CoapOptionCollection Options
        {
            get { return this._options; }
        }

        protected virtual byte TokenLength
        {
            get { return this._tokenLength; }
            set
            {
                if (value > 8)
                {
                    throw new IndexOutOfRangeException("Token length is between 0 and 8 inclusive.");
                }

                this._tokenLength = value;
            }
        }

        public virtual byte[] Encode()
        {
            LoadOptions();
            int length = 0;

            byte[] header = new byte[4 + this.TokenLength];

            int index = 0;

            header[index++] = (byte)((byte)(0x01 << 0x06) | (byte)(Convert.ToByte((int)MessageType) << 0x04) | (byte)(this.TokenLength));

            int code = (int)this.Code;
            header[index++] = code < 10 ? (byte)code : (byte)((byte)(Convert.ToByte(Convert.ToString((int)this.Code).Substring(0, 1)) << 0x05) |
                                                              (byte)(Convert.ToByte(Convert.ToString((int)this.Code).Substring(1, 2))));
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

            if (this.Payload != null)
            {
                length += this.Payload.Length + 1;
                buffer = new byte[length];
                Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
                if (options != null)
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

        public virtual void Decode(byte[] message)
        {
            int index = 0;
            byte header = message[index++];

            if (header >> 0x06 != 1)
            {
                throw new CoapVersionMismatchException("Coap Version 1 is only supported version for Coap response.");
            }

            this.MessageType = (CoapMessageType)Convert.ToInt32((header >> 0x04) & 0x03);
            this.TokenLength = (byte)(header & 0x0F);

            byte code = message[index++];
            this.Code = (CodeType)(((code >> 0x05) * 100) + (code & 0x1F));

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
                if (index < message.Length)
                {
                    marker = ((message[index] & 0xFF) == 0xFF);
                }
                else
                {
                    break;
                }
            }

            if (marker) //grab the payload
            {
                index++;
                this.Payload = new byte[message.Length - index];
                Buffer.BlockCopy(message, index, this.Payload, 0, this.Payload.Length);
            }

            this.HasContentFormat = this.Options.ContainsContentFormat();

            ReadOptions(this);            
        }
        
        protected void LoadOptions()
        {
            Action<OptionType, byte[]> loadByteArray = new Action<OptionType, byte[]>((type, array) =>
            {
                this.Options.Add(new CoapOption(type, array));
            });

            Action<OptionType, string> loadString = new Action<OptionType, string>((type, value) =>
            {
                if (value != null)
                {
                    this.Options.Add(new CoapOption(type, value));
                }
            });

            Action<OptionType, bool> loadBool = new Action<OptionType, bool>((type, value) =>
            {
                if (value)
                {
                    this.Options.Add(new CoapOption(type, null));
                }
            });

            Action<OptionType, uint, bool> loadUint = new Action<OptionType, uint, bool>((type, value, includeZero) =>
            {
                if (value > 0)
                {
                    this.Options.Add(new CoapOption(type, value));
                }

                if (value == 0 && includeZero)
                {
                    this.Options.Add(new CoapOption(type, value));
                }
            });            

            this.Options.Clear();

            if (this.ResourceUri != null)
            {
                IEnumerable<CoapOption> resourceOptions = this.ResourceUri.DecomposeCoapUri();
                foreach (CoapOption co in resourceOptions)
                {
                    this.Options.Add(co);
                }
            }

            if (this.IfMatch != null)
            {
                this.IfMatch.ForEach(s => loadByteArray(OptionType.IfMatch, s));
            }

            if (this.ETag != null)
            {
                this.ETag.ForEach(s => loadByteArray(OptionType.ETag, s));
            }

            loadBool(OptionType.IfNoneMatch, this.IfNoneMatch);

            this.LocationPath.ForEach(s => loadString(OptionType.LocationPath, s));

            if (this.ContentType.HasValue)
            {
                loadUint(OptionType.ContentFormat, (uint)this.ContentType.Value, true);
            }

            loadUint(OptionType.MaxAge, this.MaxAge, false);
            if(this.Accept.HasValue)
            {
                loadUint(OptionType.Accept, (uint)this.Accept.Value, false);
            }

            //loadUint(OptionType.Accept, this.Accept, false);
            this.LocationQuery.ForEach(s => loadString(OptionType.LocationQuery, s));
            loadString(OptionType.ProxyUri, this.ProxyUri);
            loadString(OptionType.ProxyScheme, this.ProxyScheme);
            loadUint(OptionType.Size1, this.Size1, false);
        }

        protected static void ReadOptions(CoapMessage message)
        {
            object[] ifmatch = message.Options.GetOptionValues(OptionType.IfMatch);
            object[] etag = message.Options.GetOptionValues(OptionType.ETag);
            object[] locationpath = message.Options.GetOptionValues(OptionType.LocationPath);
            object[] uripath = message.Options.GetOptionValues(OptionType.UriPath);
            object[] uriquery = message.Options.GetOptionValues(OptionType.UriQuery);
            object[] locationquery = message.Options.GetOptionValues(OptionType.LocationQuery);

            message.ResourceUri = message.Options.GetResourceUri();

            message._ifMatch = ifmatch == null ? new List<byte[]>() : new List<byte[]>(ifmatch as byte[][]);
            message._eTag = etag == null ? new List<byte[]>() : new List<byte[]>(etag as byte[][]);
            message.IfNoneMatch = message.Options.Contains(new CoapOption(OptionType.IfNoneMatch, null));
            message._locationPath = locationpath == null ? new List<string>() : new List<string>(locationpath as string[]);

            object contentType = (message.Options.GetOptionValue(OptionType.ContentFormat));
            if (contentType != null)
            {
                message.ContentType = (MediaType)Convert.ToInt32(contentType);
            }

            message.MaxAge = message.Options.GetOptionValue(OptionType.MaxAge) != null ? (uint)message.Options.GetOptionValue(OptionType.MaxAge) : 0;
            
            object accept = message.Options.GetOptionValue(OptionType.Accept);
            if(accept != null)
            {
                message.Accept = (MediaType)Convert.ToInt32(accept);
            }

            //message.Accept = message.Options.GetOptionValue(OptionType.Accept) != null ? (uint)message.Options.GetOptionValue(OptionType.Accept) : 0;
            message._locationQuery = locationquery == null ? new List<string>() : new List<string>(locationquery as string[]);
            message.ProxyUri = message.Options.GetOptionValue(OptionType.ProxyUri) as string;
            message.ProxyScheme = message.Options.GetOptionValue(OptionType.ProxyScheme) as string;
            message.Size1 = message.Options.GetOptionValue(OptionType.Size1) != null ? (uint)message.Options.GetOptionValue(OptionType.Size1) : 0;
        }

        
    }
}
