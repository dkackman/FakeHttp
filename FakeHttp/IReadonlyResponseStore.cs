using System.Threading.Tasks;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Interface to abstract retrevial of <see cref="HttpResponseMessage"/> instances
    /// </summary>
    public interface IReadonlyResponseStore
    {
        /// <summary>
        /// Find a response in the store
        /// </summary>
        /// <param name="request">A <see cref="HttpRequestMessage"/> that describes the desired response</param>
        /// <returns>A <see cref="HttpResponseMessage"/>. Will return a 404 message if no response is found</returns>
        Task<HttpResponseMessage> FindResponse(HttpRequestMessage request);

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the 
        /// <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        Task<bool> ResponseExists(HttpRequestMessage request);
    }
}
