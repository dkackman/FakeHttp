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
        /// <param name="info">The deserialized <see cref="ResponseInfo"/></param>
        void Deserialized(ResponseInfo info);

        /// <summary>
        /// Called after content is retrieved from the actual serive and before it is is saved to disk.
        /// Primarily allows for response content to mask sensitive data (ex SSN or other PII) before it is saved to storage
        /// </summary>
        /// <param name="response">The response the describes the content</param>
        /// <param name="content">The content as a byte array</param>
        /// <returns>The original byte array or a modified byte array to save to storage</returns>
        Task<Stream> Serializing(HttpResponseMessage response, Stream content);

        /// <summary>
        /// Determines if a given query paramter should be excluded from serialziation
        /// </summary>
        /// <param name="name">The name of the Uri query parameter</param>
        /// <param name="value">The value of the uri query parameter</param>
        /// <returns>True to filter the parameter. False to include in serialization and hashing</returns>
        bool FilterParameter(string name, string value);
    }
}
