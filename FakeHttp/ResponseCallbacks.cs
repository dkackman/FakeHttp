using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FakeHttp
{
    /// <summary>
    /// Default implementations of the <see cref="IResponseCallbacks"/> interface that do nothing
    /// </summary>
    public class ResponseCallbacks : IResponseCallbacks
    {
        private readonly Func<string, string, bool> _paramFilter;

        /// <summary>
        /// This ctor is only meant for backwards compatiblity with the use of the paramFilter constructors
        /// </summary>
        /// <param name="paramFilter"></param>
        [Obsolete("For backwards compatibility only. Implement IResponseCallbacks or derive from this class instead of using this constructor.")]
        public ResponseCallbacks(Func<string, string, bool> paramFilter)
        {
            if (paramFilter == null) throw new ArgumentNullException("paramFilter");

            _paramFilter = paramFilter;
        }

        /// <summary>
        /// ctor
        /// </summary>
        public ResponseCallbacks()
        {
        }

        /// <summary>
        /// Called just before the response is returned. Update deserialized values as necessary
        /// Primarily for cases where time based header values (like content expiration) need up to date values
        /// </summary>
        /// <param name="info">Desrialized response data. Header collections can be modified. Might be null if content file but no response file is present</param>
        /// <param name="content">The deserialized content stream. Might be null if response has no content</param>
        /// <returns>The original content or a modified content stream to attach to the <see cref="HttpResponseMessage"/></returns>
        public async virtual Task<Stream> Deserialized(ResponseInfo info, Stream content)
        {
            return await Task.Run(() => content); 
        }

        /// <summary>
        /// Determines if a given query parameter should be excluded from serialization
        /// </summary>
        /// <param name="name">The name of the Uri query parameter</param>
        /// <param name="value">The value of the uri query parameter</param>
        /// <returns>False</returns>
        public virtual bool FilterParameter(string name, string value)
        {
            if (_paramFilter != null)
            {
                return _paramFilter(name, value);
            }

            return false;
        }

        /// <summary>
        /// Called after content is retrieved from the actual service and before it is is saved to disk.
        /// Primarily allows for response content to mask sensitive data (ex. SSN or other PII) before it is saved to storage
        /// </summary>
        /// <param name="response">The response</param>
        /// <returns>The original content stream</returns>
        public virtual async Task<Stream> Serializing(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
