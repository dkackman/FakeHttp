using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace MockHttp.UnitTests
{
    enum MessageHandlerMode
    {
        Capture,
        Mock,
        Online
    }

    static class Factory
    {
        private const MessageHandlerMode _mode = MessageHandlerMode.Mock;

        public static HttpMessageHandler CreateMessageHandler(string mockResponseFolder, string captureFolder)
        {
            if (_mode == MessageHandlerMode.Capture)
            {
                return new CapturingHttpClientHandler(new FileSystemResponseStore(mockResponseFolder, captureFolder));
            }

            if (_mode == MessageHandlerMode.Mock)
            {
                return new MockHttpClientHandler(new FileSystemResponseStore(mockResponseFolder));
            }

            return new HttpClientHandler();
        }
    }
}
