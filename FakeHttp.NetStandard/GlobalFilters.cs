using System;
using System.Collections.Generic;

namespace FakeHttp
{
    static class GlobalFilters
    {
        public static readonly HashSet<string> SensitiveHeaderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "x-api-key",
            "fakehttp"
        };

        public static readonly HashSet<string> HeaderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fakehttp"
        };

        public static readonly HashSet<string> SensitiveParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "key",
            "apikey",
            "api-key",
            "api_key",
            "client_id",
            "client_secret",
            "access_token",
            "password",
            "pwd",
            "SAMLRequest",
            "SAMLart"
        };

        public static readonly HashSet<string> ParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
