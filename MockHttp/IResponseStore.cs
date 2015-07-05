using System.Threading.Tasks;

using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Interface to abstract storage and retrevial of <see cref="System.Net.Http.HttpResponseMessage"/> instances
    /// </summary>
    public interface IResponseStore : IReadonlyResponseStore
    {
        /// <summary>
        /// Store a <see cref="System.Net.Http.HttpResponseMessage"/> 
        /// </summary>
        /// <param name="response">The <see cref="System.Net.Http.HttpResponseMessage"/> to store</param>
        /// <returns>A Task</returns>
        Task StoreResponse(HttpResponseMessage response);
    }
}
