using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FakeHttp
{
    /// <summary>
    /// Default implementation of the <see cref="IResponseCallbacks"/> interface
    /// </summary>
    public class ResponseCallbacks : IResponseCallbacks
    {
        /// <summary>
        /// Flag indicating whether to automatically set the Date header to the current date/time on deserialization
        /// </summary>
        /// <value>True</value>
        public bool SetHeaderDate { get; set; } = true;

        /// <summary>
        /// Flag indicating whether to automatically filter commonly used header
        /// and query parameter name such as X-API-KEY, api-key, key, etc from being serialized
        /// </summary>
        public bool FilterCommonSensitiveValues { get; set; } = true;

        /// <summary>
        /// A list of header names that will not be serialized. For
        /// example x-api-key may not be something to store
        /// </summary>
        public virtual IEnumerable<string> FilteredHeaderNames => FilterCommonSensitiveValues ? GlobalFilters.SensitiveHeaderNames.Union(GlobalFilters.HeaderNames) : GlobalFilters.HeaderNames;

        /// <summary>
        /// A list of query parameter names that will not be serialized
        /// </summary>
        public virtual IEnumerable<string> FilteredParameterNames => FilterCommonSensitiveValues ? GlobalFilters.SensitiveParameterNames.Union(GlobalFilters.ParameterNames) : GlobalFilters.ParameterNames;

        /// <summary>
        /// Called just before the response is returned in order to update deserialized values as necessary.
        /// Primarily for cases where time based header values (like content expiration) need up to date values
        /// </summary>
        /// <param name="info">Deserialized response data. Header collections can be modified. Might be null if content file but no response file is present</param>
        /// <param name="content">The deserialized content stream. Might be null if response has no content</param>
        /// <returns>The original content or a modified content stream to attach to the <see cref="HttpResponseMessage"/></returns>
        public virtual Stream Deserialized(ResponseInfo info, Stream content)
        {
            if (SetHeaderDate && (info?.ResponseHeaders?.ContainsKey("Date") == true))
            {
                info.ResponseHeaders["Date"] = Enumerable.Repeat(DateTimeOffset.UtcNow.ToString("r"), 1);
            }

            return content;
        }

        /// <summary>
        /// Determines if a given query parameter should be excluded from serialization
        /// </summary>
        /// <param name="name">The name of the Uri query parameter</param>
        /// <param name="value">The value of the uri query parameter</param>
        /// <returns>True if the parameter should be filtered from serialization</returns>
        public virtual bool FilterParameter(string name, string value)
        {
            return FilteredParameterNames.Contains(name);
        }

        /// <summary>
        /// Called after content is retrieved from the actual service and before it is is saved to disk.
        /// Primarily allows for response content to mask sensitive data (ex. SSN or other PII) before it is saved to storage
        /// </summary>
        /// <param name="response">The response</param>
        /// <returns>The original content stream or a modified content stream</returns>
        public virtual async Task<Stream> Serializing(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
