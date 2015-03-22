using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;

using Newtonsoft.Json;

namespace BingGeoCoder.Client
{
    /// <summary>
    /// Concrete implementation of <see cref="BingGeoCoder.Client.IGeoCoder"/>
    /// </summary>
    public sealed class GeoCoder : IGeoCoder
    {
        private readonly int _retryCount = 5;
        private readonly int _retryDelay = 1000;

        private readonly HttpClient _httpClient;
        private readonly string _defaultParameters;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="apiKey">Bing maps API key https://msdn.microsoft.com/en-us/library/ff428642.aspx </param>
        /// <param name="user_agent">User agnet string</param>
        /// <param name="culture">Culture of the requesting application</param>
        /// <param name="context">Optional ontext of the request</param>
        public GeoCoder(string apiKey, string user_agent = "", string culture = "en-US", UserContext context = null)
            : this(apiKey, 4, 1000, user_agent, culture, context)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="apiKey">Bing maps API key https://msdn.microsoft.com/en-us/library/ff428642.aspx </param>
        /// <param name="retryCount">The number of times to retry the request if the Bing maps service indicates it is busy</param>
        /// <param name="retryDelay">The number of milliseconds to wait between retries</param>
        /// <param name="user_agent">User agnet string</param>
        /// <param name="culture">Culture of the requesting application</param>
        /// <param name="context">Optional ontext of the request</param>
        public GeoCoder(string apiKey, int retryCount, int retryDelay, string user_agent = "", string culture = "en-US", UserContext context = null)
        {
            _httpClient = Factory.CreateClient(user_agent);
            _defaultParameters = Factory.CreateDefaultParameters(apiKey, culture, context);

            _retryCount = retryCount;
            _retryDelay = retryDelay;
        }

        public async Task<string> GetAddressPart(double lat, double lon, AddressEntityType entityType)
        {
            return await GetAddressPart(lat, lon, entityType.ToString());
        }

        public async Task<string> GetAddressPart(double lat, double lon, string entityType)
        {
            var parms = new Dictionary<string, object>();
            parms.Add("includeEntityTypes", entityType);
            if (entityType.Equals("Neighborhood", StringComparison.OrdinalIgnoreCase))
            {
                parms.Add("inclnb", "1");
            }

            var result = await Get<GeoCodeResult>(string.Format("Locations/{0},{1}", lat, lon), parms);

            return result.GetFirstAddressPart(entityType);
        }

        public async Task<string> GetFormattedAddress(double lat, double lon)
        {
            var parms = new Dictionary<string, object>();
            parms.Add("includeEntityTypes", "Address,PopulatedPlace,Postcode1,AdminDivision1,CountryRegion");

            var result = await Get<GeoCodeResult>(string.Format("Locations/{0},{1}", lat, lon), parms);

            return result.GetFirstFormattedAddress();
        }

        public async Task<Address> GetAddress(double lat, double lon, bool includeNeighborhood = false)
        {
            var result = await GetGeoCodeResult(lat, lon, includeNeighborhood);

            return result.GetFirstAddress();
        }

        public async Task<GeoCodeResult> GetGeoCodeResult(double lat, double lon, bool includeNeighborhood = false)
        {
            var parms = new Dictionary<string, object>();
            parms.Add("includeEntityTypes", "Address,Neighborhood,PopulatedPlace,Postcode1,AdminDivision1,AdminDivision2,CountryRegion");
            parms.Add("inclnb", includeNeighborhood ? "1" : "0");

            return await Get<GeoCodeResult>(string.Format("Locations/{0},{1}", lat, lon), parms);
        }

        public async Task<Tuple<double, double>> QueryCoordinate(string query, int maxResults = 1)
        {
            var result = await Query(query, maxResults);

            return result.GetFirstCoordinate();
        }

        public async Task<GeoCodeResult> Query(string query, int maxResults = 1)
        {
            var parms = new Dictionary<string, object>();
            parms.Add("q", query.Replace("\n", ", "));
            parms.Add("maxRes", maxResults);

            return await Get<GeoCodeResult>("Locations", parms);
        }

        public async Task<Address> ParseAddress(string address)
        {
            var parms = new Dictionary<string, object>();
            parms.Add("q", address.Replace("\n", ", "));
            parms.Add("maxRes", 1);
            parms.Add("incl", "queryParse");

            var result = await Get<GeoCodeResult>("Locations", parms);
            return result.GetFirstAddress();
        }

        public async Task<Tuple<double, double>> GetCoordinate(string addressLine, string locality, string adminDistrict, string postalCode, string countryRegion, int maxResults = 1)
        {
            var result = await GetGeoCodeResult(addressLine, locality, adminDistrict, postalCode, countryRegion, maxResults);

            return result.GetFirstCoordinate();
        }

        public async Task<Tuple<double, double>> GetCoordinate(string landMark, int maxResults = 1)
        {
            var parms = new Dictionary<string, object>();
            parms.Add("maxRes", maxResults);

            var result = await Get<GeoCodeResult>("Locations/" + landMark, parms);

            return result.GetFirstCoordinate();
        }

        public async Task<Tuple<double, double>> GetCoordinate(Address address, int maxResults = 1)
        {
            var result = await GetGeoCodeResult(address, maxResults);

            return result.GetFirstCoordinate();
        }

        public async Task<GeoCodeResult> GetGeoCodeResult(string addressLine, string locality, string adminDistrict, string postalCode, string countryRegion, int maxResults = 1)
        {
            var parms = new Dictionary<string, object>();
            parms.Add("addressLine", addressLine);
            parms.Add("locality", locality);
            parms.Add("adminDistrict", adminDistrict);
            parms.Add("postalCode", postalCode);
            parms.Add("countryRegion", countryRegion);
            parms.Add("maxRes", maxResults);

            return await Get<GeoCodeResult>("Locations", parms);
        }

        public async Task<GeoCodeResult> GetGeoCodeResult(Address address, int maxResults = 1)
        {
            return await GetGeoCodeResult(address.addressLine, address.locality, address.adminDistrict, address.postalCode, address.countryRegion, maxResults);
        }

        private async Task<T> Get<T>(string endPoint, IDictionary<string, object> parms) where T : class
        {
            Debug.Assert(parms != null);

            Uri uri = new Uri(endPoint + _defaultParameters + parms.AsQueryString("&"), UriKind.Relative);

            for (int i = 0; i <= _retryCount; i++)
            {
                var response = await TryGetResponse(uri);
                if (response != null)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    Debug.Assert(!string.IsNullOrEmpty(content));
                    if (!string.IsNullOrEmpty(content))
                    {
                        return JsonConvert.DeserializeObject<T>(content);
                    }
                }
                else
                {
                    await Task.Delay(_retryDelay);
                }
            }

            throw new TimeoutException(string.Format("Bing service did not indicate a valid response after {0} attempts.", _retryCount));
        }

        private async Task<HttpResponseMessage> TryGetResponse(Uri uri)
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            IEnumerable<string> values = null;
            // if the bing service is overloaded it sets this header to 1 to indicate that you can retry
            if (response.Headers.TryGetValues("X-MS-BM-WS-INFO", out values) && values.Any(v => v == "1"))
            {
                return null;
            }

            return response;
        }

        /// <summary>
        /// Disposes the http client
        /// </summary>
        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}