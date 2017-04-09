using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace FakeHttp.Stores
{
    /// <summary>
    /// Resources that reside in IsolatedStorage (used for store apps
    /// </summary>
    public sealed class IsolatedStorageResources : IResources
    {
        private IsolatedStorageFile _folder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storage">The folder where resources reside</param>
        public IsolatedStorageResources(IsolatedStorageFile storage)
        {        
            _folder = storage ?? throw new ArgumentNullException("storage");
        }

        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        public async Task<bool> Exists(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(fileName)) return false;

            return await Task.Run<bool>(() =>
            {
                return _folder.FileExists(Path.Combine(folder, fileName));
            });
        }

        /// <summary>
        /// Loads a given file as a string
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file's contents as a string</returns>
        public async Task<string> LoadAsString(string folder, string fileName)
        {
            if (!await Exists(folder, fileName)) return null;

            using (var reader = new StreamReader(await LoadAsStream(folder, fileName)))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Loads a given file as a stream
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>File's contents as a stream</returns>
        public async Task<Stream> LoadAsStream(string folder, string fileName)
        {
            if (!await Exists(folder, fileName)) return null;

            return await Task.Run(() =>
                _folder.OpenFile(Path.Combine(folder, fileName), FileMode.Open, FileAccess.Read, FileShare.Read)
            );
        }
    }
}
