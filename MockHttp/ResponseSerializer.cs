using System.Net.Http;
using System.Linq;

using Newtonsoft.Json;

namespace MockHttp
{
    /// <summary>
    /// Class responsible for the serialization of <see cref="System.Net.Http.HttpResonseMessage"/> objects
    /// </summary>
    public sealed class ResponseSerializer
    {
        private readonly RequestFormatter _formatter;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="formatter"></param>
        public ResponseSerializer(RequestFormatter formatter)
        {
            _formatter = formatter;
        }

        /// <summary>
        /// Convert the <see cref="System.Net.Http.HttpResonseMessage"/> into an object
        /// that is more serialization friendly
        /// </summary>
        /// <param name="response">The <see cref="System.Net.Http.HttpResonseMessage"/></param>
        /// <returns>A serializable object</returns>
        public ResponseInfo PackageResponse(HttpResponseMessage response)
        {
            var query = _formatter.NormalizeQuery(response.RequestMessage.RequestUri);
            var fileName = _formatter.ToFileName(response.RequestMessage, query);
            var fileExtension = "";
            if (response.Content.Headers.ContentType != null)
            {
                fileExtension = MimeMap.GetFileExtension(response.Content.Headers.ContentType.MediaType);
            }

            // since HttpHeaders is not a creatable object, store the headers off to the side
            var headers = response.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var contentHeaders = response.Content.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new ResponseInfo()
            {
                StatusCode = response.StatusCode,
                Query = query,
                ContentFileName = !string.IsNullOrEmpty(fileExtension) ? string.Concat(fileName, ".content", fileExtension) : null,
                ResponseHeaders = headers,
                ContentHeaders = contentHeaders
            };
        }
    }
}
