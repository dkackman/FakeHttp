using System.Net.Http;

namespace FakeHttp
{
    static class Extensions
    {
        public static HttpResponseMessage PrepareResponse(this HttpResponseMessage response)
        {
            if (response != null)
            {
                response.Headers.TryAddWithoutValidation("FAKEHTTP", "1");
            }
            return response;
        }
    }
}
