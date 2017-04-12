using System;
using System.IO;
using System.Text;

namespace FakeHttp.Resources
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
            using (var reader = new StreamReader(LoadFromFile(folder, fileName)))
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
            // since we are passing the stream out of our control
            // disconnect it from the File in order to avoid IO locks
            using (var file = LoadFromFile(folder, fileName))
            {
                var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }

        private Stream LoadFromFile(string folder, string fileName)
        {
            return new FileStream(FullPath(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private string FullPath(string folder, string fileName)
        {
            return Path.Combine(_storeFolder, Path.Combine(folder, fileName));
        }

        public void Store(string folder, string fileName, Stream content)
        {
            Directory.CreateDirectory(Path.Combine(_storeFolder, folder));

            using (var file = new FileStream(FullPath(folder, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                content.CopyTo(file);
            }
        }

        public void Store(string folder, string fileName, string content)
        {
            Directory.CreateDirectory(Path.Combine(_storeFolder, folder));

            using (var stream = new FileStream(FullPath(folder, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            using (var responseWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                responseWriter.Write(content);
            }
        }
    }
}
