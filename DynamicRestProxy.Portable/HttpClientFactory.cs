using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.Practices.ServiceLocation;

namespace DynamicRestProxy.PortableHttpClient
{
    static class HttpClientFactory
    {
        private static bool _serviceLocationFault;

        private static HttpClientHandler GetRegisteredHandler()
        {
            if (ServiceLocator.IsLocationProviderSet && !_serviceLocationFault)
            {
                try
                {
                    return ServiceLocator.Current.GetInstance<HttpClientHandler>();
                }
                catch
                {
                    // the above will throw if no locator is registered or the handler type is not registered
                    // note that we don't have a locator so we just return the normal handler from here on out
                    // this avoids try-catch-return default which would be the most common path in non test scenarios
                    _serviceLocationFault = true;
                }
            }

            return new HttpClientHandler();
        }

        private static HttpClientHandler CreateHandler()
        {
            var handler = GetRegisteredHandler();

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return handler;
        }

        public static HttpClient CreateClient(Uri baseUri, DynamicRestClientDefaults defaults)
        {
            var handler = CreateHandler();

            var client = new HttpClient(handler, false);

            client.BaseAddress = baseUri;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/x-json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));

            if (handler.SupportsTransferEncodingChunked())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = true;
            }

            if (defaults != null)
            {
                ProductInfoHeaderValue productHeader = null;
                if (!string.IsNullOrEmpty(defaults.UserAgent) && ProductInfoHeaderValue.TryParse(defaults.UserAgent, out productHeader))
                {
                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.Add(productHeader);
                }

                foreach (var kvp in defaults.DefaultHeaders)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                if (!string.IsNullOrEmpty(defaults.AuthToken) && !string.IsNullOrEmpty(defaults.AuthScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(defaults.AuthScheme, defaults.AuthToken);
                }
            }

            return client;
        }
    }
}
