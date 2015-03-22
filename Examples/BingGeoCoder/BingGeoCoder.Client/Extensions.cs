using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace BingGeoCoder.Client
{
    static class Extensions
    {
        public static string AsQueryString(this IEnumerable<KeyValuePair<string, object>> parameters, string prepend = "?")
        {
            if (!parameters.Any())
            {
                return "";
            }

            var builder = new StringBuilder(prepend);

            var separator = "";
            foreach (var kvp in parameters.Where(kvp => kvp.Value != null))
            {
                builder.AppendFormat("{0}{1}={2}", separator, WebUtility.UrlEncode(kvp.Key), WebUtility.UrlEncode(kvp.Value.ToString()));
                separator = "&";
            }

            return builder.ToString();
        }
    }
}
