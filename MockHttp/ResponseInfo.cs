using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace MockHttp
{
    public enum SerializationFormat
    {
        Text,
        ByteArray
    }

    public sealed class ResponseInfo
    {
        public HttpResponseMessage Response { get; set; }

        public string Query { get; set; }

        public string ContentFileName { get; set; }

        public string ContentType { get; set; }

        public SerializationFormat ContentSerializationFormat { get; set; }
    }
}
