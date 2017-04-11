using System.IO;

namespace FakeHttp
{
    /// <summary>
    /// Interface to retrieve named resources that exist in a named container.
    /// </summary>
    public interface IReadOnlyResources
    {   
        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        bool Exists(string folder, string fileName);

        /// <summary>
        /// Loads a given file as a string
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file's contents as a string</returns>
        string LoadAsString(string folder, string fileName);

        /// <summary>
        /// Loads a given file as a stream
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File's contents as a stream</returns>
        Stream LoadAsStream(string folder, string fileName);
    }
}
