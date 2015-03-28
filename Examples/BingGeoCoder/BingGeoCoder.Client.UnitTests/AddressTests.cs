using System;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using BingGeoCoder.Client;

using UnitTestHelpers;

using GalaSoft.MvvmLight.Ioc;

namespace GeoCoderTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class AddressTests
    {
        private static IGeoCoder _service;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var handler = SimpleIoc.Default.GetInstance<HttpMessageHandler>();

            _service = new GeoCoder(handler, CredentialStore.RetrieveObject("bing.key.json").Key, "Portable-Bing-GeoCoder-UnitTests/1.0");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (_service != null)
            {
                _service.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetAddress()
        {
            var address = await _service.GetAddress(44.9108238220215, -93.1702041625977);

            Assert.IsNotNull(address);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task RoundtripPostalCode()
        {
            var coord = await _service.GetCoordinate(new Address() { postalCode = "55116", countryRegion = "US" });
            var address = await _service.GetAddress(coord.Item1, coord.Item2);

            Assert.AreEqual("55116", address.postalCode);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task ParseAnAddress()
        {
            var address = await _service.ParseAddress("One Microsoft Way, Redmond, WA 98052");

            Assert.AreEqual("1 Microsoft Way", address.addressLine);
            Assert.AreEqual("Redmond", address.locality);
            Assert.AreEqual("WA", address.adminDistrict);
            Assert.AreEqual("98052", address.postalCode);
            Assert.AreEqual("United States", address.countryRegion);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task ParseACanadianAddress()
        {
            var handler = SimpleIoc.Default.GetInstance<HttpMessageHandler>();
            var coord = new Tuple<double, double>(62.832908630371094, -95.913322448730469);
            using (var service = new GeoCoder(handler, CredentialStore.RetrieveObject("bing.key.json").Key, "Portable Bing GeoCoder unit tests", "en-CA", new UserContext(coord)))
            {
                var address = await service.ParseAddress("1950 Meadowvale Blvd., Mississauga, ON L5N 8L9");

                Assert.AreEqual("1950 Meadowvale Blvd", address.addressLine);
                Assert.AreEqual("Mississauga", address.locality);
                Assert.AreEqual("ON", address.adminDistrict);
                Assert.AreEqual("L5N 8L9", address.postalCode);
                Assert.AreEqual("Canada", address.countryRegion);
            }
        }
    }
}
