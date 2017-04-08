using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FakeHttp
{
    public interface IResources
    {   /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        Task<bool> Exists(string folder, string fileName);

        /// <summary>
        /// Loads a given file as a string
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file's contents as a string</returns>
        Task<string> LoadAsString(string folder, string fileName);

        /// <summary>
        /// Loads a given file as a stream
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File's contents as a stream</returns>
        Task<Stream> LoadAsStream(string folder, string fileName);
    }
}
