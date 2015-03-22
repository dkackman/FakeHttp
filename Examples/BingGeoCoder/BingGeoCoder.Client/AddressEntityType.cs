using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingGeoCoder.Client
{
    /// <summary>
    /// Enumeration indicating the various parts of an address
    /// </summary>
    public enum AddressEntityType
    {
        Address,

        Neighborhood,

        PopulatedPlace,

        Postcode1,

        AdminDivision1,

        AdminDivision2,

        CountryRegion
    }
}
