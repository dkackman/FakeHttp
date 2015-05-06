using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MockHttp
{
    public sealed class ResponseInfo
    {
        public ResponseInfo()
        {
            ResponseHeaders = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
            ContentHeaders = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public HttpStatusCode StatusCode { get; set; }

        public string Query { get; set; }

        public string ContentFileName { get; set; }

        public Dictionary<string, IEnumerable<string>> ResponseHeaders { get; set; }

        public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; }

        public HttpResponseMessage CreateResponse()
        {
            var response = new HttpResponseMessage(StatusCode);
            foreach (var kvp in ResponseHeaders)
            {
                response.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            return response;
        }

        public HttpContent CreateContent(Stream stream)
        {
            var content = new StreamContent(stream);
            foreach (var kvp in ContentHeaders)
            {
                content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            return content;
        }

        public override int GetHashCode()
        {
            return StatusCode.GetHashCode()
                ^ GetHashCode(Query)
                ^ GetHashCode(ContentFileName);
        }

        private static int GetHashCode(object o)
        {
            return o != null ? o.GetHashCode() : 0;
        }

        public override bool Equals(object obj)
        {
            ResponseInfo info = obj as ResponseInfo;
            if (info == null)
            {
                return false;
            }

            return this.StatusCode == info.StatusCode
                && this.Query == info.Query
                && this.ContentFileName == info.ContentFileName;
        }
    }
}
