﻿using System.Net;
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

        public static HttpClientHandler CreateMessageHandler(string mockResponseFolder, string captureFolder)
        {
            var handler = CreateInstance(mockResponseFolder, captureFolder);
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return handler;
        }

        private static HttpClientHandler CreateInstance(string mockResponseFolder, string captureFolder)
        {
            if (Mode == MessageHandlerMode.Capture)
            {
                return new CapturingHttpClientHandler(new FileSystemResponseStore(mockResponseFolder, captureFolder));
            }

            if (Mode == MessageHandlerMode.Mock)
            {
                return new MockHttpClientHandler(new FileSystemResponseStore(mockResponseFolder));
            }

            return new HttpClientHandler();
        }
    }
}
