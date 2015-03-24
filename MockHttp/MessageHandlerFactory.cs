using System;
using System.Net;
using System.Net.Http;

namespace MockHttp
{
    public enum MessageHandlerMode
    {
        Capture,
        Mock,
        Online
    }

    public static class MessageHandlerFactory
    {
        static MessageHandlerFactory()
        {
            Mode = MessageHandlerMode.Online;
        }

        public static MessageHandlerMode Mode { get; set; }

        public static HttpMessageHandler CreateMessageHandler(string mockResponseFolder, string captureFolder)
        {
            return CreateMessageHandler(mockResponseFolder, captureFolder, (key, value) => false);
        }


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
