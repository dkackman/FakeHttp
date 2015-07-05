using System;
using System.Net;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Flag indicating what type of <see cref="System.Net.Http.HttpMessageHandler"/> the 
    /// <see cref="FakeHttp.MessageHandlerFactory"/> will create by default
    /// </summary>
    public enum MessageHandlerMode
    {
        /// <summary>
        /// Create a handler that will retreive messages from endpoint and store for future use
        /// </summary>
        Capture,

        /// <summary>
        /// Create a handler that will retreive message from faking storage
        /// </summary>
        Fake,

        /// <summary>
        /// Create the default HttpMessage handler
        /// </summary>
        Online
    }

    /// <summary>
    /// Static factory class that creates <see cref="System.Net.Http.HttpMessageHandler"/>
    /// instances for unit tests
    /// </summary>
    public static class MessageHandlerFactory
    {
        static MessageHandlerFactory()
        {
            Mode = MessageHandlerMode.Online;
        }

        /// <summary>
        /// Controls what type of <see cref="System.Net.Http.HttpMessageHandler"/> to create by default
        /// </summary>
        public static MessageHandlerMode Mode { get; set; }

        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpMessageHandler"/>.
        /// </summary>
        /// <param name="responseStore">Object that can store and retreive response messages</param>
        /// <returns>A <see cref="System.Net.Http.HttpMessageHandler"/></returns>
        public static HttpMessageHandler CreateMessageHandler(IReadonlyResponseStore responseStore)
        {
            if (Mode == MessageHandlerMode.Fake)
            {
                return new FakeHttpMessageHandler(responseStore);
            }

            if (Mode == MessageHandlerMode.Capture)
            {
                throw new InvalidOperationException("Cannot use Capture mode with an IReadonlyResponseStore");
            }

            var clientHandler = new HttpClientHandler();

            if (clientHandler.SupportsAutomaticDecompression)
            {
                clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            return clientHandler;
        }

        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpMessageHandler"/>.
        /// </summary>
        /// <param name="responseStore">Object that can store and retreive response messages</param>
        /// <returns>A <see cref="System.Net.Http.HttpMessageHandler"/></returns>
        public static HttpMessageHandler CreateMessageHandler(IResponseStore responseStore)
        {
            if (Mode == MessageHandlerMode.Fake)
            {
                return new FakeHttpMessageHandler(responseStore);
            }

            var clientHandler = Mode == MessageHandlerMode.Capture
                ? new CapturingHttpClientHandler(responseStore) : new HttpClientHandler();

            if (clientHandler.SupportsAutomaticDecompression)
            {
                clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            return clientHandler;
        }
    }
}
