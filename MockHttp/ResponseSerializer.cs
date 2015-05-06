using System.Net.Http;
using System.Linq;

using Newtonsoft.Json;

namespace MockHttp
{
    public sealed class ResponseSerializer
    {
        private readonly RequestFormatter _formatter;

        public ResponseSerializer(RequestFormatter formatter)
        {
            _formatter = formatter;
        }

        public ResponseInfo PackageResponse(HttpResponseMessage response)
        {
            var query = _formatter.NormalizeQuery(response.RequestMessage.RequestUri);
            var fileName = _formatter.ToFileName(response.RequestMessage, query);
            var fileExtension = MimeMap.GetFileExtension(response.Content.Headers.ContentType.MediaType);

            // since HttpHeaders is not a creatable object, store the headers off to the side
            var headers = response.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var contentHeaders = response.Content.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new ResponseInfo()
            {
                StatusCode = response.StatusCode,
                Query = query,
                ContentFileName = string.Concat(fileName, ".content", fileExtension),
                ResponseHeaders = headers,
                ContentHeaders = contentHeaders
            };
        }

        public string Serialize(ResponseInfo info)
        {
            return JsonConvert.SerializeObject(info, Formatting.Indented);
        }

        public ResponseInfo Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ResponseInfo>(json);
        }
    }
}
