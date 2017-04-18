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
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <param name="content"></param>
        void Store(string folder, string fileName, string content);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <param name="content"></param>
        void Store(string folder, string fileName, Stream content);
    }
}
