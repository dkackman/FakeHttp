using System.Collections.Generic;

namespace BingGeoCoder.Client
{
    public class Address
    {
        public string countryRegion { get; set; }
        public string adminDistrict { get; set; }
        public string postalCode { get; set; }
        public string locality { get; set; }
        public string adminDistrict2 { get; set; }
        public string formattedAddress { get; set; }
        public string neighborhood { get; set; }
        public string addressLine { get; set; }
        public string landmark { get; set; }
    }

    public class Point
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }

    public class GeocodePoint
    {
        public string type { get; set; }
        public List<string> usageTypes { get; set; }
        public List<double> coordinates { get; set; }
        public string calculationMethod { get; set; }
    }

    public class Resource
    {
        public Address address { get; set; }
        public string __type { get; set; }
        public string entityType { get; set; }
        public string confidence { get; set; }
        public Point point { get; set; }
        public List<string> matchCodes { get; set; }
        public string name { get; set; }
        public List<GeocodePoint> geocodePoints { get; set; }
        public List<double> bbox { get; set; }
    }

    public class ResourceSet
    {
        public List<Resource> resources { get; set; }
        public int estimatedTotal { get; set; }
    }

    public class GeoCodeResult
    {
        public string copyright { get; set; }
        public string brandLogoUri { get; set; }
        public string traceId { get; set; }
        public string statusDescription { get; set; }
        public int statusCode { get; set; }
        public List<ResourceSet> resourceSets { get; set; }
        public string authenticationResultCode { get; set; }
    }
}
