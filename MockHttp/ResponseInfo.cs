using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace MockHttp
{
    class ResponseInfo
    {
        public HttpResponseMessage Response { get; set; }

        public string ContentFileName { get; set; }
    }
}
