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
        [Obsolete("Use constructor that takes IResponseCallbacks instead")]
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
        /// <param name="info">The deserialized <see cref="ResponseInfo"/></param>
        public virtual void Deserialized(ResponseInfo info)
        {
        }

        /// <summary>
        /// Determines if a given query paramter should be excluded from serialziation
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
        /// <param name="response">The response the describes the content</param>
        /// <param name="content">The content as a byte array</param>
        /// <returns>The original cintent byte array</returns>
        public virtual async Task<Stream> Serializing(HttpResponseMessage response, Stream content)
        {
            return await Task.Run(() => content);
        }
    }
}
