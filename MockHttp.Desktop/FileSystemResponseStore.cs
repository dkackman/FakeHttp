using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;

using MockHttp.Desktop;

namespace MockHttp
{
    public class FileSystemResponseStore : IResponseStore
    {
        private readonly RequestFormatter _formatter;
        private readonly ResponseLoader _responseLoader = new DesktopResponseLoader();

        private readonly string _storeFolder;
        private readonly string _captureFolder;

        public FileSystemResponseStore(string storeFolder)
            : this(storeFolder, storeFolder)
        {
        }

        public FileSystemResponseStore(string storeFolder, string captureFolder)
            : this(storeFolder, captureFolder, (key, value) => false)
        {

        }

        public FileSystemResponseStore(string storeFolder, Func<string, string, bool> paramFilter)
            : this(storeFolder, storeFolder, paramFilter)
        {
        }

        public FileSystemResponseStore(string storeFolder, string captureFolder, Func<string, string, bool> paramFilter)
        {
            _storeFolder = storeFolder;
            _captureFolder = captureFolder;
            _formatter = new DesktopRequestFormatter(paramFilter);
        }

        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            var query = _formatter.NormalizeQuery(request.RequestUri);
            var folderPath = Path.Combine(_storeFolder, _formatter.ToFilePath(request.RequestUri));

            // first try to find a file keyed to the request method and query
            return await _responseLoader.DeserializeResponse(folderPath, _formatter.ToFileName(request, query))
                // next just look for a default response based on just the http method
                ?? await _responseLoader.DeserializeResponse(folderPath, _formatter.ToShortFileName(request))
                // otherwise return 404            
                ?? new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
        }

        public async Task StoreResponse(HttpResponseMessage response)
        {
            var query = _formatter.NormalizeQuery(response.RequestMessage.RequestUri);
            var folderPath = Path.Combine(_captureFolder, _formatter.ToFilePath(response.RequestMessage.RequestUri));
            var fileName = _formatter.ToFileName(response.RequestMessage, query);

            Directory.CreateDirectory(folderPath);

            // this is the object that is serialized (response, normalized request query and pointer to the content file)
            var info = new ResponseInfo()
            {
                Response = response,
                Query = query,
                ContentFileName = fileName + ".content.json"
            };
            
            // just read the entire content stream as a string and serialize it 
            // we are assuming all content is json for the time being
            var content = await response.Content.ReadAsStringAsync();
            using (var contentWriter = new StreamWriter(Path.Combine(folderPath, info.ContentFileName), false))
            {
                contentWriter.Write(content);
            }

            // now serialize the response object and its meta-data
            var json = JsonConvert.SerializeObject(info, Formatting.Indented, new HttpResponseMessageConverter());
            using (var responseWriter = new StreamWriter(Path.Combine(folderPath, fileName + ".response.json"), false))
            {
                responseWriter.Write(json);
            }
        }
    }
}
