using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

namespace FakeHttp.FileSystem
{
    /// <summary>
    /// Class that can store and retrieve response messages in a win32 runtime environment
    /// </summary>
    public sealed class FileSystemResponseStore : IResponseStore
    {
        private readonly ResponseAdapter _responseAdapter;

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
        {
            if (string.IsNullOrEmpty(storeFolder)) throw new ArgumentException("storeFolder cannot be empty", "storeFolder");
            if (string.IsNullOrEmpty(captureFolder)) throw new ArgumentException("captureFolder cannot be empty", "captureFolder");
            if (callbacks == null) throw new ArgumentNullException("callbacks");

            _captureFolder = captureFolder;
            _responseAdapter = new ResponseAdapter(new FileSystemResources(storeFolder), callbacks);
        }

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the 
        /// <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        public async Task<bool> ResponseExists(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return await _responseAdapter.Exists(request);
        }

        /// <summary>
        /// Retrieve response message from storage based on a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            return await _responseAdapter.FindResponse(request);
        }

        /// <summary>
        /// Stores a response message for later retrieval
        /// </summary>
        /// <param name="response">The response message to store</param>
        /// <returns>Task</returns>
        public async Task StoreResponse(HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException("response");

            var folderPath = Path.Combine(_captureFolder, _responseAdapter.Formatter.ToResourcePath(response.RequestMessage.RequestUri));
            var fileName = _responseAdapter.Formatter.ToName(response.RequestMessage, _responseAdapter.RepsonseCallbacks.FilterParameter);

            // this is the object that is serialized (response, normalized request query and pointer to the content file)
            var info = _responseAdapter.Formatter.PackageResponse(response, _responseAdapter.RepsonseCallbacks.FilterParameter);

            Directory.CreateDirectory(folderPath);
            // just read the entire content stream and serialize it 
            using (var file = new FileStream(Path.Combine(folderPath, info.ContentFileName), FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await response.Content.LoadIntoBufferAsync();

                var content = await _responseAdapter.RepsonseCallbacks.Serializing(response);

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
