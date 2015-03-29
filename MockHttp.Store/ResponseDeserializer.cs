using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

using Windows.Storage.Streams;

using Newtonsoft.Json;

using Windows.Storage;

namespace MockHttp.Store
{
    class ResponseDeserializer
    {
        public async Task<HttpResponseMessage> DeserializeResponse(IStorageFolder folder, string fileName)
        {
            var fullName = fileName + ".response.json";

            var file = await folder.TryGetFile(fullName);
            // first look for a completely serialized response
            if (file != null)
            {
                using (var stream = await file.OpenSequentialReadAsync())
                using (var reader = new DataReader(stream))
                {
                    var json = await reader.ReadToEndAsync();                    

                    var info = JsonConvert.DeserializeObject<ResponseInfo>(json);
                    if (info == null)
                    {
                        return null;
                    }

                    if (!string.IsNullOrEmpty(info.ContentFileName))
                    {
                        info.Response.Content = await DeserializeContent(folder, info.ContentFileName);
                    }
                    return info.Response;
                }
            }

            // no fully serialized response exists just look for a content file
            return await DeserializeFromContent(folder, fileName);
        }

        private async Task<HttpResponseMessage> DeserializeFromContent(IStorageFolder folder, string fileName)
        {
            var fullName = fileName + ".content.json";

            var file = await folder.TryGetFile(fullName);
            // first look for a completely serialized response
            if (file != null)
            {
                // no serialized response but we have serialized content
                // craft a response and attach content
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = await DeserializeContent(folder, fullName)
                };
            }

            return null;
        }

        private async Task<HttpContent> DeserializeContent(IStorageFolder folder, string fileName)
        {
            var file = await folder.TryGetFile(fileName);
            if (file != null)
            {
                using (var stream = await file.OpenSequentialReadAsync())
                using (var reader = new DataReader(stream))
                {
                    var content = await reader.ReadToEndAsync();

                    return new StringContent(content);
                }
            }

            return null;
        }
    }
}
