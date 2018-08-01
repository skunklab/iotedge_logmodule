

namespace SkunkLab.Protocols.Coap
{
    using System;

    public class CoapToken
    {
        private static Random ran;
        public CoapToken()
        {
        }

        static CoapToken()
        {
            ran = new Random();
        }

        public CoapToken(byte[] tokenBytes)
        {
            this.TokenBytes = tokenBytes;
        }
        public static CoapToken Create()
        {
            byte[] buffer = new byte[8];            
            ran.NextBytes(buffer);
            return new CoapToken(buffer);
        }
        
        public byte[] TokenBytes { get; set; }

        public string TokenString
        {
            get
            {
                return Convert.ToBase64String(this.TokenBytes);
            }
        }
    }
}
