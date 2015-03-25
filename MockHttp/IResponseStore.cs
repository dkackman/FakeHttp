using System.Threading.Tasks;

using System.Net.Http;

namespace MockHttp
{
    /// <summary>
    /// Interface to abstract storage and retrevial of <see cref="System.Net.Http.HttpResponseMessage"/> instances
    /// </summary>
    public interface IResponseStore
    {
        /// <summary>
        /// Find a response in the store
        /// </summary>
        /// <param name="request">A <see cref="System.Net.Http.HttpRequestMessage"/> that describes the desired response</param>
        /// <returns>A <see cref="System.Net.Http.HttpResponseMessage"/>. Will return a 404 message if no response is found</returns>
        Task<HttpResponseMessage> FindResponse(HttpRequestMessage request);

        /// <summary>
        /// Store a <see cref="System.Net.Http.HttpResponseMessage"/> 
        /// </summary>
        /// <param name="response">The <see cref="System.Net.Http.HttpResponseMessage"/> to store</param>
        /// <returns>A Task</returns>
        Task StoreResponse(HttpResponseMessage response);
    }
}
