using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

namespace FakeHttp.Stores
{
    /// <summary>
    /// Class that can store and retrieve response messages using <see cref="System.IO.File"/>
    /// </summary>
    public sealed class FileSystemResponseStore : ReadonlyResponseStore, IResponseStore
    {
        private readonly string _captureFolder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        public FileSystemResponseStore(string storeFolder)
            : this(storeFolder, storeFolder)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="captureFolder">folder to store captued response messages</param>
        public FileSystemResponseStore(string storeFolder, string captureFolder)
            : this(storeFolder, captureFolder, new ResponseCallbacks())
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="callbacks">Object to manage responses at runtime</param>
        public FileSystemResponseStore(string storeFolder, IResponseCallbacks callbacks)
            : this(storeFolder, storeFolder, callbacks)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="captureFolder">folder to store captued response messages</param>
        /// <param name="callbacks">Object to manage responses at runtime</param>
        public FileSystemResponseStore(string storeFolder, string captureFolder, IResponseCallbacks callbacks)
            : base(new FileSystemResources(storeFolder), callbacks)
        {
            if (string.IsNullOrEmpty(captureFolder)) throw new ArgumentException("captureFolder cannot be empty", "captureFolder");

            _captureFolder = captureFolder;
        }

        /// <summary>
        /// Stores a response message for later retrieval
        /// </summary>
        /// <param name="response">The response message to store</param>
        /// <returns>Task</returns>
        public async Task StoreResponse(HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException("response");

            var folderPath = Path.Combine(_captureFolder, Adapter.Formatter.ToResourcePath(response.RequestMessage.RequestUri));
            var fileName = Adapter.Formatter.ToName(response.RequestMessage, Adapter.RepsonseCallbacks.FilterParameter);

            // this is the object that is serialized (response, normalized request query and pointer to the content file)
            var info = Adapter.Formatter.PackageResponse(response, Adapter.RepsonseCallbacks.FilterParameter);

            Directory.CreateDirectory(folderPath);
            // just read the entire content stream and serialize it 
            using (var file = new FileStream(Path.Combine(folderPath, info.ContentFileName), FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await response.Content.LoadIntoBufferAsync();

                var content = await Adapter.RepsonseCallbacks.Serializing(response);

                await content.CopyToAsync(file);
            }

            // now serialize the response object and its meta-data
            var json = JsonConvert.SerializeObject(info, Formatting.Indented, new VersionConverter());
            using (var stream = new FileStream(Path.Combine(folderPath, fileName + ".response.json"), FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var responseWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                responseWriter.Write(json);
            }
        }
    }
}
