using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Callback interface to allow tests to supply runtime logic for responses 
    /// </summary>
    public interface IResponseCallbacks
    {
        /// <summary>
        /// Called just before the response is returned. Update deserialized values as necessary
        /// Primarily for cases where time based header values (like content expiration) need up to date values
        /// </summary>
        /// <param name="info">Desrialized response data. Header collections can be modified. Might be null if content file but no response file is present</param>
        /// <param name="content">The deserialized content stream. Might be null if response has no content</param>
        /// <returns>The original content or a modified content stream to attach to the <see cref="HttpResponseMessage"/></returns>
        Task<Stream> Deserialized(ResponseInfo info, Stream content);

        /// <summary>
        /// Called after content is retrieved from the actual service during capturing and before it is saved to disk.
        /// Primarily allows for response content to mask sensitive data (ex SSN or other PII) before it is saved to storage
        /// </summary>
        /// <param name="response">The service response</param>
        /// <returns>The original content or a modified content stream to save to storage</returns>
        Task<Stream> Serializing(HttpResponseMessage response);

        /// <summary>
        /// Determines if a given query parameter should be excluded from serialization
        /// </summary>
        /// <param name="name">The name of the Uri query parameter</param>
        /// <param name="value">The value of the uri query parameter</param>
        /// <returns>True to filter the parameter. False to include in serialization and hashing</returns>
        bool FilterParameter(string name, string value);

        /// <summary>
        /// Flag indicating whether to automatically set the Date header to the current date/time on deserialization
        /// </summary>
        bool SetHeaderDate { get; set; }
    }
}
