﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

using Newtonsoft.Json;

using Microsoft.Practices.ServiceLocation;

namespace BingGeoCoder.Client
{
    static class Factory
    {
        private static HttpMessageHandler GetHandler(out bool dispose)
        {
            if (ServiceLocator.IsLocationProviderSet)
            {
                var handler = ServiceLocator.Current.GetInstance<HttpMessageHandler>();
                if (handler != null)
                {
                    dispose = false;
                    return handler;
                }
            }

            var clientHandler = new HttpClientHandler();
            if (clientHandler.SupportsAutomaticDecompression)
            {
                clientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            dispose = true;
            return clientHandler;
        }

        public static HttpClient CreateClient(string user_agent)
        {
            bool dispose;
            var handler = GetHandler(out dispose);

            var client = new HttpClient(handler, dispose);

            if (handler is HttpClientHandler && ((HttpClientHandler)handler).SupportsTransferEncodingChunked())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = true;
            }

            client.BaseAddress = new Uri("http://dev.virtualearth.net/REST/v1/", UriKind.Absolute);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ProductInfoHeaderValue productHeader = null;
            if (!string.IsNullOrEmpty(user_agent) && ProductInfoHeaderValue.TryParse(user_agent, out productHeader))
            {
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.UserAgent.Add(productHeader);
            }

            return client;
        }

        public static string CreateDefaultParameters(string key, string culture, UserContext context)
        {
            var d = new Dictionary<string, object>();

            d.Add("key", key);

            if (!string.IsNullOrEmpty(culture))
            {
                d.Add("c", culture);
            }

            if (context != null)
            {
                if (!string.IsNullOrEmpty(context.IPAddress))
                {
                    d.Add("ip", context.IPAddress);
                }

                if (context.Location != null)
                {
                    d.Add("ul", string.Format("{0},{1}", context.Location.Item1, context.Location.Item2));
                }

                if (context.MapView != null)
                {
                    d.Add("umv", string.Format("{0},{1},{2},{3}", context.MapView.Item1, context.MapView.Item2, context.MapView.Item3, context.MapView.Item4));
                }
            }

            return d.AsQueryString();
        }
    }
}