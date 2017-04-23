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
        /// <exception cref="ArgumentException">If storeFolder is null or empty</exception>
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
        /// <returns>The file's contents as a <see cref="String"/></returns>
        /// <exception cref="FileLoadException"/>
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
        /// <returns>The file's contents as a <see cref="Stream"/></returns>
        /// <exception cref="FileLoadException"/>
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

        /// <summary>
        /// Stores a data stream in the corresponding folder and file 
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <param name="data">The data to store</param>
        /// <exception cref="IOException"/>
        public void Store(string folder, string fileName, Stream data)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(_storeFolder, folder));

                using (var file = new FileStream(FullPath(folder, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    data.CopyTo(file);
                }
            }
            catch (Exception e)
            {
                throw new IOException("An error occured atempting to store " + FullPath(folder, fileName), e);
            }
        }

        /// <summary>
        /// Stores a data string in the corresponding folder and file 
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <param name="data">The data to store</param>
        /// <exception cref="IOException"/>
        public void Store(string folder, string fileName, string data)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                Store(folder, fileName, stream);
            }
        }

        private Stream LoadFromFile(string folder, string fileName)
        {
            try
            {
                return new FileStream(FullPath(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                throw new FileLoadException(FullPath(folder, fileName) + " could not be opened", e);
            }
        }

        private string FullPath(string folder, string fileName)
        {
            return Path.Combine(_storeFolder, Path.Combine(folder, fileName));
        }
    }
}
