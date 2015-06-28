using System;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

using Newtonsoft.Json;

using MockHttp.Desktop;

namespace MockHttp
{
    /// <summary>
    /// Class that can store and retreive response messages in a win32 runtime environment
    /// </summary>
    public class FileSystemResponseStore : IResponseStore
    {
        private readonly MessageFormatter _formatter;
        private readonly ResponseLoader _responseLoader;

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
            : this(storeFolder, captureFolder, (key, value) => false)
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="paramFilter">call back used to determine if a given query paramters should be excluded from serialziation</param>
        public FileSystemResponseStore(string storeFolder, Func<string, string, bool> paramFilter)
            : this(storeFolder, storeFolder, paramFilter)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="storeFolder">root folder for storage</param>
        /// <param name="captureFolder">folder to store captued response messages</param>
        /// <param name="paramFilter">call back used to determine if a given query paramters should be excluded from serialziation</param>
        public FileSystemResponseStore(string storeFolder, string captureFolder, Func<string, string, bool> paramFilter)
        {
            _captureFolder = captureFolder;
            _formatter = new DesktopMessagetFormatter(paramFilter);
            _responseLoader = new DesktopResponseLoader(storeFolder, _formatter);
        }

        /// <summary>
        /// Retreive response message from storage based on the a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            return await _responseLoader.FindResponse(request);
        }

        /// <summary>
        /// Stores a response message for later retreival
        /// </summary>
        /// <param name="response">The response message to store</param>
        /// <returns>Task</returns>
        public async Task StoreResponse(HttpResponseMessage response)
        {
            var query = _formatter.NormalizeQuery(response.RequestMessage.RequestUri);
            var folderPath = Path.Combine(_captureFolder, _formatter.ToFolderPath(response.RequestMessage.RequestUri));
            var fileName = _formatter.ToFileName(response.RequestMessage, query);

            Directory.CreateDirectory(folderPath);

            // this is the object that is serialized (response, normalized request query and pointer to the content file)
            var info = _formatter.PackageResponse(response);

            // just read the entire content stream serialize it 
            using (var file = File.Create(Path.Combine(folderPath, info.ContentFileName)))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                file.Write(bytes, 0, bytes.Length);
            }

            // now serialize the response object and its meta-data
            var json = JsonConvert.SerializeObject(info, Formatting.Indented);
            using (var responseWriter = new StreamWriter(Path.Combine(folderPath, fileName + ".response.json"), false))
            {
                responseWriter.Write(json);
            }
        }
    }
}
