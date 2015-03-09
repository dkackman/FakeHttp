using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Http;

namespace MockHttp
{
    public interface IResponseStore
    {
        Task<HttpResponseMessage> FindResponse(HttpRequestMessage request);
    }
}
