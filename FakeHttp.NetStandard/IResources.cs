using System.IO;

namespace FakeHttp
{
    /// <summary>
    /// A reource type that can store new response messages at runtime
    /// </summary>
    public interface IResources : IReadOnlyResources
    {
        /// <summary>
        /// Stores the data string in the corresponding folder and file 
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <param name="data">The data to store</param>
        void Store(string folder, string fileName, string data);

        /// <summary>
        /// Stores the data stream in the corresponding folder and file 
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <param name="data">The data to store</param>
        void Store(string folder, string fileName, Stream data);
    }
}
