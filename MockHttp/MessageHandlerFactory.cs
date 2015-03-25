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
        /// <param name="mockResponseFolder">Path to the folder where mock response are stored (used when in Mock mode)</param>
        /// <param name="captureFolder">Path to the folder to store captured responses (used when in capture mode)</param>
        /// <returns>A <see cref="System.Net.Http.HttpMessageHandler"/></returns>
        public static HttpMessageHandler CreateMessageHandler(string mockResponseFolder, string captureFolder)
        {
            return CreateMessageHandler(mockResponseFolder, captureFolder, (name, value) => false);
        }

        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpMessageHandler"/>.
        /// </summary>
        /// <param name="mockResponseFolder">Path to the folder where mock response are stored (used when in Mock mode)</param>
        /// <param name="captureFolder">Path to the folder to store captured responses (used when in capture mode)</param>
        /// <param name="paramFilter">A callback function that allows tests to filter query parameters out of serialization</param>
        /// <returns>A <see cref="System.Net.Http.HttpMessageHandler"/></returns>
        public static HttpMessageHandler CreateMessageHandler(string mockResponseFolder, string captureFolder, Func<string, string, bool> paramFilter)
        {
            if (Mode == MessageHandlerMode.Mock)
            {
                return new MockHttpMessageHandler(new FileSystemResponseStore(mockResponseFolder, paramFilter));
            }

            var clientHandler = Mode == MessageHandlerMode.Capture
                ? new CapturingHttpClientHandler(new FileSystemResponseStore(mockResponseFolder, captureFolder, paramFilter)) : new HttpClientHandler();

            if (clientHandler.SupportsAutomaticDecompression)
            {
                clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            return clientHandler;
        }
    }
}
