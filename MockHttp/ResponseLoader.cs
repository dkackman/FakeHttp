using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;

namespace MockHttp
{
    public abstract class ResponseLoader
    {
        protected abstract Task<bool> Exists(string folder, string fileName);

        protected abstract Task<string> Load(string folder, string fileName);

        public async Task<HttpResponseMessage> DeserializeResponse(string folder, string baseName)
        {
            var fileName = baseName + ".response.json";
            // first look for a completely serialized response
            if (await Exists(folder, fileName))
            {
                var json = await Load(folder, fileName);

                var info = JsonConvert.DeserializeObject<ResponseInfo>(json);
                if (info == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(info.ContentFileName))
                {
                    info.Response.Content = await DeserializeContent(folder, baseName + ".content.json");
                }
                return info.Response;
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
                var content = await Load(folder, fileName);

                return new StringContent(content);
            }

            return null;
        }
    }
}
