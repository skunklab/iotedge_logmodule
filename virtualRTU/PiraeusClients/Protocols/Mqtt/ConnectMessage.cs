
namespace SkunkLab.Protocols.Mqtt
{
    using System;
    using System.Diagnostics;
    using System.Text;

    public class ConnectMessage : MqttMessage
    {
        public ConnectMessage()
        {           
        }

        public ConnectMessage(string clientId, bool cleanSession)
        {
            this.ClientId = clientId;
            this.CleanSession = cleanSession;
        }

        public ConnectMessage(string clientId, int keepAliveSeconds, bool cleanSession)
        {
            this.ClientId = clientId;
            this.KeepAlive = keepAliveSeconds;
            this.CleanSession = cleanSession;
        }

        public ConnectMessage(string clientId, string username, string password, int keepAliveSeconds, bool cleanSession)       
        {
            this.ClientId = clientId;
            this.Username = username;
            this.Password = password;
            this.KeepAlive = keepAliveSeconds;
            this.CleanSession = cleanSession;
        }

        public ConnectMessage(string clientId,
                             QualityOfServiceLevelType willQualityOfServiceLevel,
                             string willTopic, string willMessage, bool willRetain, bool cleanSession)
            : this(clientId, null, null, 0, willQualityOfServiceLevel, willTopic, willMessage, willRetain, cleanSession)
        {
        }

        public ConnectMessage(string clientId, int keepAliveSeconds, 
                             QualityOfServiceLevelType willQualityOfServiceLevel,
                             string willTopic, string willMessage, bool willRetain, bool cleanSession)
            : this(clientId, null, null, keepAliveSeconds, willQualityOfServiceLevel, willTopic, willMessage, willRetain, cleanSession)
        {
        }

        public ConnectMessage(string clientId, string username, string password, int keepAliveSeconds, 
                             QualityOfServiceLevelType willQualityOfServiceLevel,
                             string willTopic, string willMessage, bool willRetain, bool cleanSession)
        {
            this.ClientId = clientId;
            this.Username = username;
            this.Password = password;
            this.KeepAlive = keepAliveSeconds;
            this.WillFlag = true;
            this.WillQualityOfServiceLevel = willQualityOfServiceLevel;
            this.WillTopic = willTopic;
            this.WillMessage = willMessage;
            this.WillRetain = willRetain;
            this.CleanSession = cleanSession;
        }

        private int _version = 0x04;
        private string _protocolName = "MQTT";

        public override bool HasAck
        {
            get { return true; }
        }

        public override MqttMessageType MessageType
        {
            get
            {
                return MqttMessageType.CONNECT;
            }

            internal set
            {
                base.MessageType = value;
            }
        }


        public string Username { get; internal set; }

        public string Password { get; internal set; }

        public string ClientId { get; internal set; }

        public string WillTopic { get; internal set; }

        public string WillMessage { get; internal set; }

        public string ProtocolName
        {
            get { return _protocolName; } //return "MQIsdp"; }
            set { _protocolName = value; }
        }

        public int ProtocolVersion
        {
            get { return _version; }
            set { _version = value; }
        }

        public bool WillFlag { get; internal set; }

        public QualityOfServiceLevelType? WillQualityOfServiceLevel { get; internal set; }

        public bool CleanSession { get; internal set; }

        public int KeepAlive { get; internal set; }

        public bool WillRetain { get; internal set; }

        private bool usernameFlag;
        private bool passwordFlag;
        private byte willQoS;
        //private bool willRetain;
        private byte connectFlags;

        
        private void SetConnectFlags()
        {
            usernameFlag = !string.IsNullOrEmpty(this.Username);
            passwordFlag = !string.IsNullOrEmpty(this.Password);

            if (passwordFlag && !usernameFlag)
            {
                //fault
            }

            if (this.WillFlag && ((string.IsNullOrEmpty(this.WillTopic) || string.IsNullOrEmpty(this.WillMessage)) || !this.WillQualityOfServiceLevel.HasValue))
            {
                //fault
            }

            

            willQoS = 0x00;
            if (this.WillQualityOfServiceLevel.HasValue)
            {
                willQoS = (byte)(int)this.WillQualityOfServiceLevel;
                //willQoS = Convert.ToByte((int)Enum.Parse(typeof(QualityOfServiceLevelType), this.WillQualityOfServiceLevel.Value.ToString(), false));
            }

            this.connectFlags = 0x00;

            this.connectFlags |= this.usernameFlag ? (byte)(0x01 << 0x07) : (byte)0x00;
            this.connectFlags |= this.passwordFlag ? (byte)(0x01 << 0x06) : (byte)0x00;
            this.connectFlags |= this.WillRetain ? (byte)(0x01 << 5) : (byte)0x00;
            this.connectFlags |= (byte)(willQoS << 0x03);
            this.connectFlags |= this.WillFlag ? (byte)(0x01 << 0x02) : (byte)0x00;
            this.connectFlags |= this.CleanSession ? (byte)(0x01 << 0x01) : (byte)0x00;
        }
        

        //public ConnectMessage(ConnectVariableHeader variableHeader, ConnectPayload payload)
        //{
        //    this.VariableHeader = variableHeader;
        //    this.Payload = payload;
        //}

        public override byte[] Encode()
        {
            byte fixedHeader = (byte)((0x01 << Constants.Header.MessageTypeOffset) |
                   (byte)(0x00 << Constants.Header.QosLevelOffset) |
                   (byte)(0x00 << Constants.Header.DupFlagOffset) |
                   (byte)(0x00));

            //ConnectVariableHeader variableHeader = this.VariableHeader as ConnectVariableHeader;
            //ConnectPayload payload = this.Payload as ConnectPayload;
            //variableHeader.SetConnectFlags(payload);

            SetConnectFlags();
            ByteContainer vhContainer = new ByteContainer();
            vhContainer.Add(this.ProtocolName);
            vhContainer.Add((byte)this.ProtocolVersion);
            vhContainer.Add(this.connectFlags);

            byte[] keepAlive = new byte[2];
            keepAlive[0] = (byte)((this.KeepAlive >> 8) & 0x00FF); // MSB
            keepAlive[1] = (byte)(this.KeepAlive & 0x00FF); // LSB

            vhContainer.Add(keepAlive);

            byte[] variableHeaderBytes = vhContainer.ToBytes();

            ByteContainer payloadContainer = new ByteContainer();
            payloadContainer.Add(this.ClientId);
            payloadContainer.Add(this.WillTopic);
            payloadContainer.Add(this.WillMessage);
            payloadContainer.Add(this.Username);
            payloadContainer.Add(this.Password);

            byte[] payloadBytes = payloadContainer.ToBytes();

            //byte[] variableHeaderBytes = variableHeader.Encode();
            //byte[] payloadBytes = payload.Encode();

            int remainingLength = variableHeaderBytes.Length + payloadBytes.Length;
            byte[] remainingLengthBytes = base.EncodeRemainingLength(remainingLength);

            ByteContainer container = new ByteContainer();
            container.Add(fixedHeader);
            container.Add(remainingLengthBytes);
            container.Add(variableHeaderBytes);
            container.Add(payloadBytes);

            return container.ToBytes();
        }

        internal override MqttMessage Decode(byte[] message)
        {
            MqttMessage connectMessage = new ConnectMessage();

            int index = 0;
            byte fixedHeader = message[index];
            base.DecodeFixedHeader(fixedHeader);
            
            
            int remainingLength = base.DecodeRemainingLength(message);

            int temp = remainingLength; //increase the fixed header size
            do
            {
                index++;
                temp = temp / 128;
            } while (temp > 0);

            index++;

            byte[] buffer = new byte[remainingLength];
            Buffer.BlockCopy(message, index, buffer, 0, buffer.Length);

            index = 0;

            int protocolNameLength = ((buffer[index++] << 8) & 0xFF00);
            protocolNameLength |= buffer[index++];

            byte[] protocolName = new byte[protocolNameLength];
            try
            {
                Buffer.BlockCopy(buffer, index, protocolName, 0, protocolNameLength);
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

            _protocolName = Encoding.UTF8.GetString(protocolName);

            //if (Encoding.UTF8.GetString(protocolName) != this.ProtocolName)
            //{
                //fault wrong protocol
            //}

            index += protocolNameLength;
            this.ProtocolVersion = (int)buffer[index++];
            //if (this.ProtocolVersion != (int)buffer[index++])
            //{
            //    //fault wrong version
            //}

            byte connectFlags = buffer[index++];
            this.usernameFlag = ((connectFlags >> 0x07) == 0x01) ? true : false;
            this.passwordFlag = ((connectFlags & 0x64) >> 0x06 == 0x01) ? true : false;
            this.WillRetain = ((connectFlags & 0x032) >> 0x05 == 0x01) ? true : false;

            this.WillQualityOfServiceLevel = (QualityOfServiceLevelType)Convert.ToByte(((connectFlags & 0x1F) >> 0x03) | (connectFlags & 0x08 >> 0x03));
            this.WillFlag = ((connectFlags & 0x04) >> 0x02 == 0x01) ? true : false;
            this.CleanSession = ((connectFlags & 0x02) >> 0x01 == 0x01) ? true : false;

            int keepAliveSec = ((buffer[index++] << 8) & 0xFF00);
            keepAliveSec |= buffer[index++];

            this.KeepAlive = keepAliveSec;


            int length = 0;
            this.ClientId = ByteContainer.DecodeString(buffer, index, out length);
            index += length;

            if (this.WillFlag)
            {
                this.WillTopic = ByteContainer.DecodeString(buffer, index, out length);
                index += length;
                this.WillMessage = ByteContainer.DecodeString(buffer, index, out length);
                index += length;
            }

            if (this.usernameFlag)
            {
                this.Username = ByteContainer.DecodeString(buffer, index, out length);
                index += length;
            }

            if (this.passwordFlag)
            {
                this.Password = ByteContainer.DecodeString(buffer, index, out length);
                index += length;
            }
            
            return connectMessage;
        }
    }
}
