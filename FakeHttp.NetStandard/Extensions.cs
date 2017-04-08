using System.Net.Http;
using System.Diagnostics;

namespace FakeHttp
{
    static class Extensions
    {
        public static HttpResponseMessage PrepareResponse(this HttpResponseMessage response)
        {
            Debug.Assert(response != null);

            response.Headers.TryAddWithoutValidation("FAKEHTTP", "1");

            return response;
        }
    }
}
