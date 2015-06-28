using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace MockHttp
{
    /// <summary>
    /// Base calss for file based response message retreival that allows 
    /// separation between desktop and universal app machanisms for file api
    /// </summary>
    public abstract class ResponseLoader
    {
        private readonly MessageFormatter _formatter;

        /// <summary>
        /// ctor 
        /// </summary>
        /// <param name="formatter">PLatofrma specific formatter object</param>
        protected ResponseLoader(MessageFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// Checks whether the specified file exists
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns>Flag indicating whether file exists</returns>
        protected abstract Task<bool> Exists(string folder, string fileName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns></returns>
        protected abstract Task<string> LoadAsString(string folder, string fileName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder">The folder name</param>
        /// <param name="fileName">The file name</param>
        /// <returns></returns>
        protected abstract Task<Stream> LoadAsStream(string folder, string fileName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="baseName"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeserializeResponse(string folder, string baseName)
        {
            var fileName = baseName + ".response.json";
            // first look for a completely serialized response
            if (await Exists(folder, fileName))
            {
                var json = await LoadAsString(folder, fileName);
                var info = JsonConvert.DeserializeObject<ResponseInfo>(json);

                if (info == null)
                {
                    return null;
                }

                var response = info.CreateResponse();
                if (!string.IsNullOrEmpty(info.ContentFileName))
                {
                    response.Content = await LoadContent(folder, info.ContentFileName);
                }
                return response;
            }

            // no fully serialized response exists just look for a content file
            return await CreateResponseFromContent(folder, baseName);
        }

        private async Task<HttpResponseMessage> CreateResponseFromContent(string folder, string baseName)
        {
            var fileName = baseName + ".content.json"; // only json supported as raw content right now
            if (await Exists(folder, fileName))
            {
                // no serialized response but we have serialized content
                // craft a response and attach content
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = await LoadContent(folder, fileName)
                };
            }

            return null;
        }

        private async Task<HttpContent> LoadContent(string folder, string fileName)
        {
            if (await Exists(folder, fileName))
            {
                var stream = await LoadAsStream(folder, fileName);
                return new StreamContent(stream);
            }

            return null;
        }
    }
}
