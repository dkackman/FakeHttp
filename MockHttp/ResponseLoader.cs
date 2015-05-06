using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockHttp
{
    public abstract class ResponseLoader
    {
        private readonly ResponseSerializer _serializer;

        protected ResponseLoader(ResponseSerializer serializer)
        {
            _serializer = serializer;
        }

        protected abstract Task<bool> Exists(string folder, string fileName);

        protected abstract Task<string> LoadJson(string folder, string fileName);

        protected abstract Task<Stream> GetContentStream(string folder, string fileName);

        public async Task<HttpResponseMessage> DeserializeResponse(string folder, string baseName)
        {
            var fileName = baseName + ".response.json";
            // first look for a completely serialized response
            if (await Exists(folder, fileName))
            {               
                var json = await LoadJson(folder, fileName);
                var info = _serializer.Deserialize(json);
                if (info == null)
                {
                    return null;
                }

                var response = info.CreateResponse();
                if (!string.IsNullOrEmpty(info.ContentFileName))
                {
                    response.Content = await DeserializeContent(folder, baseName + ".content.json");
                }
                return response;
            }

            // no fully serialized response exists just look for a content file
            return await DeserializeFromContent(folder, fileName);
        }

        private async Task<HttpResponseMessage> DeserializeFromContent(string folder, string fileName)
        {
            if (await Exists(folder, fileName))
            {
                // no serialized response but we have serialized content
                // craft a response and attach content
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = await DeserializeContent(folder, fileName)
                };
            }

            return null;
        }

        private async Task<HttpContent> DeserializeContent(string folder, string fileName)
        {
            if (await Exists(folder, fileName))
            {
                var stream = await GetContentStream(folder, fileName);
                return new StreamContent(stream);
            }

            return null;
        }
    }
}
