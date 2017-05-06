using System;
using System.Net;
using System.Net.Http;

namespace FakeHttp
{
    /// <summary>
    /// Flag indicating what type of <see cref="HttpMessageHandler"/> the 
    /// <see cref="MessageHandlerFactory"/> will create by default
    /// </summary>
    public enum MessageHandlerMode
    {
        /// <summary>
        /// Create a handler that will retrieve messages from endpoint and store for future use
        /// (Always calls the endpoint)
        /// </summary>
        Capture,

        /// <summary>
        /// Create a handler that will retrieve message from faking storage
        /// </summary>
        Fake,

        /// <summary>
        /// Create a <see cref="HttpClientHandler"/>
        /// </summary>
        Online,

        /// <summary>
        /// Create a handler that will use stored responses if they exist. 
        /// If they do not exist the handler will retrieve them from the online endpoint and store for future use
        /// (Calls the endpoint only if no local response is found)
        /// </summary>                
        Automatic
    }

    /// <summary>
    /// Static factory class that creates <see cref="HttpMessageHandler"/> instances for unit tests
    /// </summary>
    public static class MessageHandlerFactory
    {
        /// <summary>
        /// Controls what type of <see cref="HttpMessageHandler"/> to create by default
        /// </summary>
        public static MessageHandlerMode Mode { get; set; } = MessageHandlerMode.Automatic;

        /// <summary>
        /// Create an <see cref="System.Net.Http.HttpMessageHandler"/>.
        /// </summary>
        /// <param name="resources">Instance where faked responses are stored</param>
        /// <returns>A <see cref="System.Net.Http.HttpMessageHandler"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public static HttpMessageHandler CreateMessageHandler(IReadOnlyResources resources)
        {
            return CreateMessageHandler(resources, new ResponseCallbacks());
        }

        /// <summary>
        /// Create an <see cref="HttpMessageHandler"/>.
        /// </summary>
        /// <param name="resources">Instance where faked responses are stored</param>
        /// <param name="callbacks"></param>
        /// <returns>A <see cref="HttpMessageHandler"/> appropriate for the current <see cref="Mode"/></returns>
        /// <exception cref="InvalidOperationException">If <see cref="Mode"/> is <see cref="MessageHandlerMode.Capture"/> 
        /// or <see cref="MessageHandlerMode.Automatic"/></exception>
        /// <exception cref="ArgumentNullException"/>
        public static HttpMessageHandler CreateMessageHandler(IReadOnlyResources resources, IResponseCallbacks callbacks)
        {
            if (resources == null) throw new ArgumentNullException("resources");
            if (callbacks == null) throw new ArgumentNullException("callbacks");

            if (Mode == MessageHandlerMode.Fake)
            {
                return new FakeHttpMessageHandler(new ReadOnlyResponseStore(resources, callbacks));
            }

            if (Mode == MessageHandlerMode.Capture || Mode == MessageHandlerMode.Automatic)
            {
                throw new InvalidOperationException("Cannot use Capture or Automatic mode with an IReadOnlyResources");
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
        /// Create an <see cref="HttpMessageHandler"/>.
        /// </summary>
        /// <param name="resources">Instance where faked responses are stored</param>
        /// <returns>A <see cref="HttpMessageHandler"/> appropriate for the current <see cref="Mode"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public static HttpMessageHandler CreateMessageHandler(IResources resources)
        {
            return CreateMessageHandler(resources, new ResponseCallbacks());
        }

        /// <summary>
        /// Create an <see cref="HttpMessageHandler"/>.
        /// </summary>
        /// <param name="resources">Instance where faked responses are stored</param>
        /// <param name="callbacks"></param>
        /// <returns>A <see cref="HttpMessageHandler"/> appropriate for the current <see cref="Mode"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public static HttpMessageHandler CreateMessageHandler(IResources resources, IResponseCallbacks callbacks)
        {
            if (resources == null) throw new ArgumentNullException("resources");
            if (callbacks == null) throw new ArgumentNullException("callbacks");

            var store = new ResponseStore(resources, callbacks);

            // fake has a different base class than capture and automatic
            // so thats why this is up here
            if (Mode == MessageHandlerMode.Fake)
            {
                return new FakeHttpMessageHandler(store);
            }

            var clientHandler =
                Mode == MessageHandlerMode.Capture ? new CapturingHttpClientHandler(store) :
                Mode == MessageHandlerMode.Automatic ? new AutomaticHttpClientHandler(store) :
                new HttpClientHandler();

            if (clientHandler.SupportsAutomaticDecompression)
            {
                clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            return clientHandler;
        }
    }
}
