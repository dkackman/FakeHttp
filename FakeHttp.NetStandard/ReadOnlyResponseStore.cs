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
        /// <summary>
        /// Object used to format folder and file names for storage
        /// </summary>
        internal readonly MessageFormatter _formatter;

        private readonly IReadOnlyResources _resources;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources">An instance that manages the underlying storage of response resources</param>
        /// <exception cref="ArgumentNullException"/>
        public ReadOnlyResponseStore(IReadOnlyResources resources)
            : this(resources, new ResponseCallbacks())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resources">An instance that manages the underlying storage of response resources</param>
        /// <param name="callbacks">Object used to allow client code to modify responses during load and storage</param>
        /// <exception cref="ArgumentNullException"/>
        public ReadOnlyResponseStore(IReadOnlyResources resources, IResponseCallbacks callbacks)
        {
            _resources = resources ?? throw new ArgumentNullException("loader");

            _formatter = new MessageFormatter(callbacks);
        }

        /// <summary>
        /// Determines if a <see cref="HttpResponseMessage"/> exists for the <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns>True if a response exists for the request. Otherwise false</returns>
        /// <exception cref="ArgumentNullException"/>
        public bool ResponseExists(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var folderPath = _formatter.ToResourcePath(request.RequestUri);
            var longName = _formatter.ToName(request);
            var shortName = _formatter.ToShortName(request);

            return _resources.Exists(folderPath, longName + ".response.json") || _resources.Exists(folderPath, shortName + ".response.json");
        }

        /// <summary>
        /// Retrieve response message from storage based on a request message
        /// </summary>
        /// <param name="request">The request message</param>
        /// <returns>The response messsage assoicated with the request. Returns a 404 message if none is found.</returns>
        /// <exception cref="ArgumentNullException"/>
        public HttpResponseMessage FindResponse(HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var folderPath = _formatter.ToResourcePath(request.RequestUri);
            var longName = _formatter.ToName(request);
            var shortName = _formatter.ToShortName(request);

            // first try to find a file keyed to the request method and query
            var response = DeserializeResponse(request, folderPath, longName)
                // next just look for a default response based on just the http method
                ?? DeserializeResponse(request, folderPath, shortName)
                // otherwise return 404            
                ?? Create404(request, folderPath, longName, shortName);

            response.Headers.TryAddWithoutValidation("FAKEHTTP", "1");
            return response;
        }

        private static HttpResponseMessage Create404(HttpRequestMessage request, string folderPath, string longName, string shortName)
        {
            var msg = $"Did not find response for verb {request.Method} and uri {request.RequestUri} in {folderPath}." +
                $"\nTried content and response files with base names {longName} and {shortName}." +
                "\nCheck that fake responses are copied to the unit test location.";

            Debug.WriteLine(msg);
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                RequestMessage = request,
                Content = new StringContent(msg)
            };
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
                var content = _formatter.Callbacks.Deserialized(info, stream);

                return info.CreateResponse(request, content);
            }

            // no fully serialized response exists, just look for a content file
            return CreateResponseFromContent(request, folder, baseName);
        }

        private HttpResponseMessage CreateResponseFromContent(HttpRequestMessage request, string folder, string baseName)
        {
            var fileName = baseName + ".content.json"; // only json supported as raw content right now

            if (_resources.Exists(folder, fileName))
            {
                var stream = _resources.LoadAsStream(folder, fileName);

                Debug.WriteLine($"Creating content only response for {folder} {fileName}");

                var content = _formatter.Callbacks.Deserialized(null, stream);

                // no serialized response but we have serialized content
                // craft a response and attach content
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(content),
                    RequestMessage = request
                };
            }

            Debug.WriteLine($"No response found for {folder} {baseName}");
            return null;
        }
    }
}
