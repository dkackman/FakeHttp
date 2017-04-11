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
        private readonly IReadOnlyResources _resources;
        private readonly IResponseCallbacks _callbacks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callbacks"></param>
        /// <param name="resources"></param>
        public ResponseAdapter(IReadOnlyResources resources, IResponseCallbacks callbacks)
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
        public bool Exists(HttpRequestMessage request)
        {
            var folderPath = _formatter.ToResourcePath(request.RequestUri);
            var longName = _formatter.ToName(request, _callbacks.FilterParameter);
            var shortName = _formatter.ToShortName(request);

            return _resources.Exists(folderPath, longName + ".response.json") || _resources.Exists(folderPath, shortName + ".response.json");
        }

        /// <summary>
        /// Finds the response message keyed to a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response message or a 404 message if not found</returns>
        public HttpResponseMessage FindResponse(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var folderPath = _formatter.ToResourcePath(request.RequestUri);
            var longName = _formatter.ToName(request, _callbacks.FilterParameter);
            var shortName = _formatter.ToShortName(request);

            // first try to find a file keyed to the request method and query
            var response = DeserializeResponse(request, folderPath, longName)
                // next just look for a default response based on just the http method
                ?? DeserializeResponse(request, folderPath, shortName)
                // otherwise return 404            
                ?? Create404(request, folderPath, longName, shortName);

            return response.Prepare();
        }

        private static HttpResponseMessage Create404(HttpRequestMessage request, string folderPath, string longName, string shortName)
        {
            Debug.WriteLine("Did not find response for verb {0} and uri {1} in folder {2}.", request.Method, request.RequestUri, folderPath);
            Debug.WriteLine("\tTried content and response files with bases names {0} and {1}.\n\tCheck that fake responses are copied to unit test location.", longName, shortName);

            return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
        }

        private HttpResponseMessage DeserializeResponse(HttpRequestMessage request, string folder, string baseName)
        {
            var fileName = baseName + ".response.json";
            // first look for a completely serialized response
            if (_resources.Exists(folder, fileName))
            {
                Debug.WriteLine($"Creating response from {Path.Combine(folder, fileName)}");

                var json = _resources.LoadAsString(folder, fileName);
                var info = JsonConvert.DeserializeObject<ResponseInfo>(json) ?? throw new InvalidDataException("The response exists but could not be deserialized");

                var stream = _resources.Exists(folder, info.ContentFileName) ? _resources.LoadAsStream(folder, info.ContentFileName) : null;
                var content = _callbacks.Deserialized(info, stream);

                return info.CreateResponse(request, content);
            }

            // no fully serialized response exists just look for a content file
            return CreateResponseFromContent(request, folder, baseName);
        }

        private HttpResponseMessage CreateResponseFromContent(HttpRequestMessage request, string folder, string baseName)
        {
            var fileName = baseName + ".content.json"; // only json supported as raw content right now

            if (_resources.Exists(folder, fileName))
            {
                var stream = _resources.LoadAsStream(folder, fileName);
                if (stream != null)
                {
                    Debug.WriteLine($"Creating response from {Path.Combine(folder, fileName)}");

                    var content = _callbacks.Deserialized(null, stream);

                    // no serialized response but we have serialized content
                    // craft a response and attach content
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(content),
                        RequestMessage = request
                    };
                }
            }

            Debug.WriteLine($"No response found for {Path.Combine(folder, baseName)}");
            return null;

        }
    }
}