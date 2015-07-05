using System.Threading.Tasks;

using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Interface to abstract retrevial of <see cref="System.Net.Http.HttpResponseMessage"/> instances
    /// </summary>
    public interface IReadonlyResponseStore
    {
        /// <summary>
        /// Find a response in the store
        /// </summary>
        /// <param name="request">A <see cref="System.Net.Http.HttpRequestMessage"/> that describes the desired response</param>
        /// <returns>A <see cref="System.Net.Http.HttpResponseMessage"/>. Will return a 404 message if no response is found</returns>
        Task<HttpResponseMessage> FindResponse(HttpRequestMessage request);
    }
}
