using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace BingGeoCoder.Client
{
    /// <summary>
    /// Helper methods for spelunking the GeoResultSet and its child collections
    /// </summary>
    public static class GeoResultExtensions
    {
        public static IEnumerable<Address> GetAddresses(this GeoCodeResult result)
        {
            try
            {
                return from rs in result.resourceSets
                       from r in rs.resources
                       select r.address;
            }
            catch (Exception)
            {
                return Enumerable.Empty<Address>();
            }
        }

        public static IEnumerable<Tuple<double, double>> GetCoordinates(this GeoCodeResult result)
        {
            try
            {
                return from rs in result.resourceSets
                       from r in rs.resources
                       let pts = r.point.coordinates
                       select new Tuple<double, double>(pts[0], pts[1]);
            }
            catch (Exception)
            {
                return Enumerable.Empty<Tuple<double, double>>();
            }
        }

        public static Address GetFirstAddress(this GeoCodeResult result)
        {
            try
            {
                return result.GetAddresses().FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Tuple<double, double> GetFirstCoordinate(this GeoCodeResult result)
        {
            try
            {
                return result.GetCoordinates().FirstOrDefault() ?? new Tuple<double, double>(0, 0);
            }
            catch (Exception)
            {
                return new Tuple<double, double>(0, 0);
            }
        }

        public static string GetFirstAddressProperty(this GeoCodeResult result, Func<Address, string> selector)
        {
            Debug.Assert(selector != null);

            try
            {
                var address = result.GetFirstAddress();
                if (address != null)
                    return selector(address);
            }
            catch (Exception)
            {
            }

            return "";
        }

        public static string GetFirstFormattedAddress(this GeoCodeResult result)
        {
            try
            {
                return result.GetFirstAddressProperty(a => a.formattedAddress);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetFirstAddressPart(this GeoCodeResult result, string part)
        {
            try
            {
                return result.GetFirstAddressProperty(GetAddressPartSelector(part));
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static Func<Address, string> GetAddressPartSelector(string part)
        {
            if (part.Equals("address", StringComparison.OrdinalIgnoreCase))
                return a => a.addressLine;

            if (part.Equals("Neighborhood", StringComparison.OrdinalIgnoreCase))
                return a => a.neighborhood;

            if (part.Equals("postcode1", StringComparison.OrdinalIgnoreCase))
                return a => a.postalCode;

            if (part.Equals("PopulatedPlace", StringComparison.OrdinalIgnoreCase))
                return a => a.locality;

            if (part.Equals("AdminDivision1", StringComparison.OrdinalIgnoreCase))
                return a => a.adminDistrict;

            if (part.Equals("AdminDivision2", StringComparison.OrdinalIgnoreCase))
                return a => a.adminDistrict2;

            if (part.Equals("CountryRegion", StringComparison.OrdinalIgnoreCase))
                return a => a.countryRegion;

            Debug.Assert(false);
            return a => "";
        }
    }
}
