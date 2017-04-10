using System;
using System.IO;

namespace FakeHttp.Stores
{
    /// <summary>
    /// Resources stores on the file system and accesible via <see cref="System.IO.File"/>
    /// </summary>
    public sealed class FileSystemResources : IResources
    {
        private readonly string _storeFolder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeFolder">The root folder where resources reside</param>
        public FileSystemResources(string storeFolder)
        {
            if (string.IsNullOrEmpty(storeFolder)) throw new ArgumentException("storeFolder cannot be empty", "storeFolder");

            _storeFolder = storeFolder;
        }

        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        public bool Exists(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName)) return false;

            return File.Exists(FullPath(folder, fileName));
        }

        /// <summary>
        /// Loads a given file as a string
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file's contents as a string</returns>
        public string LoadAsString(string folder, string fileName)
        {
            using (var reader = new StreamReader(LoadAsStream(folder, fileName)))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Loads a given file as a stream
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File's contents as a stream</returns>
        /// <returns></returns>
        public Stream LoadAsStream(string folder, string fileName)
        {
            return new FileStream(FullPath(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private string FullPath(string folder, string fileName)
        {
            return Path.Combine(_storeFolder, Path.Combine(folder, fileName));
        }
    }
}
