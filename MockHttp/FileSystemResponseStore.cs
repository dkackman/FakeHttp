using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;

namespace MockHttp
{
    public class FileSystemResponseStore : IResponseStore
    {
        private readonly string _storeFolder;
        private readonly string _captureFolder;
        private readonly ResponseDeserializer _deserializer = new ResponseDeserializer();

        public FileSystemResponseStore(string storeFolder)
            : this(storeFolder, storeFolder)
        {
        }

        public FileSystemResponseStore(string storeFolder, string captureFolder)
        {
            _storeFolder = storeFolder;
            _captureFolder = captureFolder;
        }

        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            var response = await _deserializer.DeserializeResponse(request.Method.ToString(), Path.Combine(_storeFolder, request.RequestUri.ToFilePath()));

            // if we find a json file that matches the request uri and method
            // deserialize it into the repsonse
            if (response != null)
            {
                return response;
            }

            // otherwise return 404            
            return await Task.Run(() =>
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    RequestMessage = request
                });
        }

        public async Task StoreResponse(HttpResponseMessage response)
        {
            var path = Path.Combine(_captureFolder, response.RequestMessage.RequestUri.ToFilePath());
            Directory.CreateDirectory(path);

            var method = response.RequestMessage.Method.ToString();

            var info = new ResponseInfo()
            {
                Response = response,
                ContentFileName = method + ".content.json"
            };

            var content = await response.Content.ReadAsStringAsync();
            using (var contentWriter = new StreamWriter(Path.Combine(path, info.ContentFileName), false))
            {
                contentWriter.Write(content);
            }

            // to avoid serializing things like api keys and auth tokens
            // null the original request object. 
            // we also don't deserialize the content object as that will be constructed sepearately on deserializaiton
            var request = response.RequestMessage;
            var oldContent = response.Content;

            info.Response.Content = null;
            info.Response.RequestMessage = null;

            var json = JsonConvert.SerializeObject(info);

            // put things back so we are a good citizen in the handler chain
            info.Response.Content = oldContent;
            info.Response.RequestMessage = request; 

            using (var responseWriter = new StreamWriter(Path.Combine(path, method + ".response.json"), false))
            {
                responseWriter.Write(json);
            }
        }
    }
}
