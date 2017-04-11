using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace FakeHttp
{
    /// <summary>
    /// A reource type that can store new response messages at runtime
    /// </summary>
    public interface IResources : IReadOnlyResources
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        void Store(string folder, string fileName, string content);

        void Store(string folder, string fileName, Stream content);
    }
}
