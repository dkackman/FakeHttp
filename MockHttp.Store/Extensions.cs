using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Windows.Storage;
using Windows.Storage.Streams;

namespace MockHttp.Store
{
    static class Extensions
    {
        public static async Task<StorageFile> TryGetFile(this IStorageFolder folder, string fileName)
        {
            try
            {
                return await folder.GetFileAsync(fileName);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        public static async Task<string> ReadToEndAsync(this DataReader reader)
        {
            const uint chunkSize = 4096;

            var builder = new StringBuilder();
            uint numBytes = await reader.LoadAsync(chunkSize);
            while (numBytes > 0)
            {
                builder.Append(reader.ReadString(numBytes));
                // Load the next chunk into the DataReader buffer.
                numBytes = await reader.LoadAsync(chunkSize);
            } 

            return builder.ToString();
        }
    }
}
