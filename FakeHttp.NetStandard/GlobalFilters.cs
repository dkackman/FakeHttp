using System;
using System.Collections.Generic;

namespace FakeHttp
{
    /// <summary>
    /// Sets of header and parameter names to filter out of response hashing and serialization.
    /// Used to prevent ephemeral values types from changing the hashed name of a response and
    /// to prevent sensitive information like api keys or auth tokens from being stored.
    /// </summary>
    public static class GlobalFilters
    {
        /// <summary>
        /// A list of names that represent senstive data and will prevent specific headers from being serialized
        /// </summary>
        public static HashSet<string> SensitiveHeaderNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Proxy-Authorization",
            "x-api-key"
        };

        /// <summary>
        /// A list of names that will prevent specific headers from being serialized
        /// </summary>
        public static HashSet<string> HeaderNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fakehttp" // this is a non-serialized sentinal value for debugging
        };

        /// <summary>
        /// A list of parameter names that represent senstive data and will prevent them from being 
        /// being serialized or included as part of responses' hashed names 
        /// </summary>
        public static HashSet<string> SensitiveParameterNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "key",
            "apikey",
            "api-key",
            "api_key",
            "client_id",
            "client_secret",
            "access_token",
            "refresh_token",
            "password",
            "pwd",
            "SAMLRequest",
            "SAMLart"
        };

        /// <summary>
        /// A list of parameter names that will prevent them from being 
        /// being serialized or included as part of responses' hashed names
        /// </summary>
        public static HashSet<string> ParameterNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "nonce"
        };
    }
}
