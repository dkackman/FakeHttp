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
        /// Create a handler that will retrieve messages from endpoint and store for future use
        /// </summary>
        Capture,

        /// <summary>
        /// Create a handler that will retrieve message from faking storage
        /// </summary>
        Fake,

        /// <summary>
        /// Create the default HttpMessage handler
        /// </summary>
        Online,

        /// <summary>
        /// Create a handler that will use stored responses if they exist. 
        /// If they do not exist the handler will retrieve them from the online endpoint and store for future use
        /// </summary>                
        Automatic
    }

    /// <summary>
    /// Static factory class that creates <see cref="System.Net.Http.HttpMessageHandler"/>
    /// instances for unit tests
    /// </summary>
    public static class MessageHandlerFactory
    {
        static MessageHandlerFactory()
        {
            Mode = MessageHandlerMode.Automatic;
        }

        /// <summary>
        /// Controls what type of <see cref="System.Net.Http.HttpMessageHandler"/> to create by default
        /// </summary>
        public static MessageHandlerMode Mode { get; set; }

        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpMessageHandler"/>.
        /// </summary>
        /// <param name="responseStore">Object that can store and retrieve response messages</param>
        /// <returns>A <see cref="System.Net.Http.HttpMessageHandler"/></returns>
        public static HttpMessageHandler CreateMessageHandler(IReadOnlyResponseStore responseStore)
        {
            if (Mode == MessageHandlerMode.Fake)
            {
                return new FakeHttpMessageHandler(responseStore);
            }

            if (Mode == MessageHandlerMode.Capture || Mode == MessageHandlerMode.Automatic)
            {
                throw new InvalidOperationException("Cannot use Capture or Automatic mode with an IReadonlyResponseStore");
            }

            // else online
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
        /// <param name="responseStore">Object that can store and retrieve response messages</param>
        /// <returns>A <see cref="System.Net.Http.HttpMessageHandler"/></returns>
        public static HttpMessageHandler CreateMessageHandler(IResponseStore responseStore)
        {
            if (Mode == MessageHandlerMode.Fake)
            {
                return new FakeHttpMessageHandler(responseStore);
            }

            var clientHandler =
                Mode == MessageHandlerMode.Capture ? new CapturingHttpClientHandler(responseStore) :
                Mode == MessageHandlerMode.Automatic ? new AutomaticHttpClientHandler(responseStore) :
                new HttpClientHandler();

            if (clientHandler.SupportsAutomaticDecompression)
            {
                clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            return clientHandler;
        }
    }
}
