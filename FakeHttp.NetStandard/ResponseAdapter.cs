using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;

namespace FakeHttp
{
    /// <summary>
    /// Adapts an <see cref="HttpResponseMessage"/> to a <see cref="IResources"/>
    /// </summary>
    public sealed class ResponseAdapter
    {
        private readonly MessageFormatter _formatter = new MessageFormatter();
        private readonly IResources _resources;
        private readonly IResponseCallbacks _callbacks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callbacks"></param>
        /// <param name="resources"></param>
        public ResponseAdapter(IResources resources, IResponseCallbacks callbacks)
        {
            _resources = resources ?? throw new ArgumentNullException("loader");
            _callbacks = callbacks ?? throw new ArgumentNullException("callbacks");
        }

        /// <summary>
        /// 
        /// </summary>
        public IResponseCallbacks RepsonseCallbacks => _callbacks;

        /// <summary>
        /// 
        /// </summary>
        public MessageFormatter Formatter => _formatter;

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the 
        /// <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        public async Task<bool> Exists(HttpRequestMessage request)
        {
            var folderPath = _formatter.ToResourcePath(request.RequestUri);

            var longName = _formatter.ToName(request, _callbacks.FilterParameter);
            var shortName = _formatter.ToShortName(request);

            return await _resources.Exists(folderPath, longName + ".response.json") || await _resources.Exists(folderPath, shortName + ".response.json");
        }

        /// <summary>
        /// Finds the response message keyed to a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response message or a 404 message if not found</returns>
        public async Task<HttpResponseMessage> FindResponse(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var folderPath = _formatter.ToResourcePath(request.RequestUri);

            var longName = _formatter.ToName(request, _callbacks.FilterParameter);
            var shortName = _formatter.ToShortName(request);

            // first try to find a file keyed to the request method and query
            var response = await DeserializeResponse(request, folderPath, longName)
                // next just look for a default response based on just the http method
                ?? await DeserializeResponse(request, folderPath, shortName)
                // otherwise return 404            
                ?? await Create404(request, folderPath, longName, shortName);

            return response.Prepare();
        }

        private static async Task<HttpResponseMessage> Create404(HttpRequestMessage request, string folderPath, string longName, string shortName)
        {
            Debug.WriteLine("Did not find response for verb {0} and uri {1} in folder {2}.", request.Method, request.RequestUri, folderPath);
            Debug.WriteLine("\tTried content and response files with bases names {0} and {1}.\n\tCheck that fake responses are copied to unit test location.", longName, shortName);

            return await Task.Run(() => new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request });
        }

        private async Task<HttpResponseMessage> DeserializeResponse(HttpRequestMessage request, string folder, string baseName)
        {
            var fileName = baseName + ".response.json";
            // first look for a completely serialized response
            if (await _resources.Exists(folder, fileName))
            {
                Debug.WriteLine($"Creating response from {Path.Combine(folder, fileName)}");

                var json = await _resources.LoadAsString(folder, fileName);
                var info = JsonConvert.DeserializeObject<ResponseInfo>(json);

                if (info == null)
                {
                    return null;
                }

                var content = await _callbacks.Deserialized(info, await _resources.LoadAsStream(folder, info.ContentFileName));

                return info.CreateResponse(request, content);
            }

            // no fully serialized response exists just look for a content file
            return await CreateResponseFromContent(request, folder, baseName);
        }

        private async Task<HttpResponseMessage> CreateResponseFromContent(HttpRequestMessage request, string folder, string baseName)
        {
            var fileName = baseName + ".content.json"; // only json supported as raw content right now

            var stream = await _resources.LoadAsStream(folder, fileName);
            if (stream != null)
            {
                Debug.WriteLine($"Creating response from {Path.Combine(folder, fileName)}");

                var content = await _callbacks.Deserialized(null, stream);

                // no serialized response but we have serialized content
                // craft a response and attach content
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(content),
                    RequestMessage = request
                };
            }

            Debug.WriteLine($"No response found for {Path.Combine(folder, baseName)}");
            return null;
        }
    }
}
