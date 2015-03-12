using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;

namespace MockHttp
{
    public class FileSystemResponseStore : IResponseStore
    {
        private readonly string _storeFolder;
        private readonly string _captureFolder;

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
            var path = Path.Combine(_storeFolder, request.RequestUri.ToFilePath());
            path = Path.Combine(path, request.Method.ToString() + ".json");

            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var content = await reader.ReadToEndAsync();

                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.RequestMessage = request;
                    response.Content = new StringContent(content);
                    return response;
                }
            }
            else
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                response.RequestMessage = request;
                return await Task.Run(() => response);
            }
        }

        public async Task StoreResponse(HttpResponseMessage response)
        {
            var path = Path.Combine(_captureFolder, response.RequestMessage.RequestUri.ToFilePath());
            Directory.CreateDirectory(path);

            path = Path.Combine(path, response.RequestMessage.Method.ToString() + ".json");

            var content = await response.Content.ReadAsStringAsync();
            using(var writer = new StreamWriter(path, false))
            {
                writer.Write(content);
            }
        }
    }
}
