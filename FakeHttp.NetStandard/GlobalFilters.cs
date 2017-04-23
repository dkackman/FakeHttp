using System;
using System.Collections.Generic;

namespace FakeHttp
{
    static class GlobalFilters
    {
        public static HashSet<string> SensitiveHeaderNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "x-api-key",
            "fakehttp"
        };

        public static HashSet<string> HeaderNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fakehttp"
        };

        public static HashSet<string> SensitiveParameterNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        public static HashSet<string> ParameterNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
