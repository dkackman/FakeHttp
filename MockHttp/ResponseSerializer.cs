using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace MockHttp
{
    public class ResponseSerializer
    {
        private readonly RequestFormatter _formatter;

        public ResponseSerializer(RequestFormatter formatter)
        {
            _formatter = formatter;
        }
    }
}
