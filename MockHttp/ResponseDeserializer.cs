using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Newtonsoft.Json;

namespace MockHttp
{
    class ResponseDeserializer
    {
        public async Task<HttpResponseMessage> DeserializeResponse(string method, string folder)
        {
            var path = Path.Combine(folder, method + ".response.json");

            // first look for a complete serialized response
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var json = await reader.ReadToEndAsync();

                    var info = JsonConvert.DeserializeObject<ResponseInfo>(json );
                    info.Response.Content = await DeserializeContent(Path.Combine(folder, info.ContentFileName));
                    return info.Response;
                }
            }

            return await DeserializeFromContent(method, folder);
        }

        private async Task<HttpResponseMessage> DeserializeFromContent(string method, string folder)
        {
            var path = Path.Combine(folder, method + ".response.json");

            if (File.Exists(path))
            {
                // no serialized response but we have serialized content
                // craft a response and attach content
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = await DeserializeContent(path)
                };
            }

            return null;
        }

        private async Task<HttpContent> DeserializeContent(string path)
        {
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var content = await reader.ReadToEndAsync();

                    return new StringContent(content);
                }
            }

            return null;
        }
    }
}
