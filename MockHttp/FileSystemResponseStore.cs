using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;

namespace MockHttp
{
    class FileSystemResponseStore : IResponseStore
    {
        private readonly string _rootFolder;

        public FileSystemResponseStore(string rootFolder)
        {
            _rootFolder = rootFolder;
        }

        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            var path = Path.Combine(_rootFolder, request.RequestUri.Host, request.RequestUri.LocalPath.TrimStart('/').Replace('/','\\'), "content.json");

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
    }
}
