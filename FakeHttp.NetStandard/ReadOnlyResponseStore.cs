using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

using Newtonsoft.Json;

namespace FakeHttp
{
    /// <summary>
    /// A store that can serve responses from an <see cref="IReadOnlyResources"/> instance
    /// </summary>
    public class ReadOnlyResponseStore : IReadOnlyResponseStore 
    {
        protected readonly MessageFormatter _formatter = new MessageFormatter();
        protected readonly IResponseCallbacks _callbacks;

        private readonly IReadOnlyResources _resources;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        public ReadOnlyResponseStore(IReadOnlyResources resources)
            : this(resources, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="callbacks"></param>
        public ReadOnlyResponseStore(IReadOnlyResources resources, IResponseCallbacks callbacks)
        {
            _resources = resources ?? throw new ArgumentNullException("loader");
            _callbacks = callbacks ?? throw new ArgumentNullException("callbacks");
        }

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the 
        /// <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        public bool ResponseExists(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var folderPath = _formatter.ToResourcePath(request.RequestUri);
            var longName = _formatter.ToName(request, _callbacks.FilterParameter);
            var shortName = _formatter.ToShortName(request);

            return _resources.Exists(folderPath, longName + ".response.json") || _resources.Exists(folderPath, shortName + ".response.json");
        }

        /// <summary>
        /// Retrieve response message from storage based on a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage</returns>
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
            Debug.WriteLine($"Did not find response for verb {request.Method} and uri {request.RequestUri} in {folderPath}.");
            Debug.WriteLine($"\tTried content and response files with bases names {longName} and {shortName}.");
            Debug.WriteLine($"\tCheck that fake responses are copied to the unit test location.");

            return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
        }

        private HttpResponseMessage DeserializeResponse(HttpRequestMessage request, string folder, string baseName)
        {
            var fileName = baseName + ".response.json";
            // first look for a completely serialized response
            if (_resources.Exists(folder, fileName))
            {
                Debug.WriteLine($"Creating response for {folder} {fileName}");

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
                    Debug.WriteLine($"Creating response for {folder} {fileName}");

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

            Debug.WriteLine($"No response found for {folder} {baseName}");
            return null;
        }
    }
}
