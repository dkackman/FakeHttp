using System;
using System.Net;
using System.Net.Http;

namespace MockHttp
{
    /// <summary>
    /// Flag indicating what type of <see cref="System.Net.Http.HttpMessageHandler"/> the 
    /// <see cref="MockHttp.MessageHandlerFactory"/> will create by default
    /// </summary>
    public enum MessageHandlerMode
    {
        Capture,
        Mock,
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
        public static HttpMessageHandler CreateMessageHandler(IResponseStore responseStore)
        {
            if (Mode == MessageHandlerMode.Mock)
            {
                return new MockHttpMessageHandler(responseStore);
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
