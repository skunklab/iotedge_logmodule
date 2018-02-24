//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Net.Http;

//namespace SkunkLab.Channels
//{
//    public abstract class ParameterReader
//    {
//        public static ParameterReader Create(HttpRequestMessage request)
//        {
//            HttpParameterReader httpReader = new HttpParameterReader(request);
//            httpReader.Read();            
//            return httpReader;
//        }

//        public static ParameterReader Create(string uriString)
//        {
//            UriParameterReader reader =  new UriParameterReader(uriString);
//            reader.Read();
//            return reader;
//        }

//        public virtual string Resource { get; internal set; }
//        public virtual string ContentType { get; internal set; }
//        public virtual string MessageId { get; internal set; }
//        public virtual IEnumerable<KeyValuePair<string, string>> Indexes { get; internal set; }
//        public virtual IEnumerable<string> Subscriptions { get; internal set; }
//        public virtual string SecurityToken { get; internal set; }
//        public virtual string TokenType { get; internal set; }
//        public abstract void Read();

//        [EditorBrowsable(EditorBrowsableState.Never)]
//        internal void BuildIndexes(IEnumerable<string> indexes)
//        {
//            List<KeyValuePair<string, string>> indexList = new List<KeyValuePair<string, string>>();
//            foreach (string index in indexes)
//            {
//                string[] parts = index.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
//                if (parts.Length != 2)
//                {
//                    throw new IndexOutOfRangeException("indexes");
//                }
//                else
//                {
//                    indexList.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
//                }
//            }

//            this.Indexes = indexList.Count > 0 ? indexList.ToArray() : null;

//        }

//        [EditorBrowsable(EditorBrowsableState.Never)]
//        internal void CheckUri(IEnumerable<string> uriStrings)
//        {
//            if (uriStrings == null)
//            {
//                return;
//            }

//            foreach (string uriString in uriStrings)
//            {
//                CheckUri(uriString);
//            }
//        }

//        [EditorBrowsable(EditorBrowsableState.Never)]
//        internal void CheckUri(string uriString)
//        {
//            if (string.IsNullOrEmpty(uriString))
//            {
//                return;
//            }

//            if (!Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
//            {
//                throw new UriFormatException("uriString");
//            }
//        }
//    }
//}
