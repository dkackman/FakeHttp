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
    [DeploymentItem("FakeResponses.zip")]
    public class CoordinateTests
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
        public async Task CoordinateFromPostalCode()
        {
            var coord = await _service.GetCoordinate(null, null, null, "55116", "US");

            Assert.IsTrue(coord.Item1.AboutEqual(44.9025726318359));
            Assert.IsTrue(coord.Item2.AboutEqual(-93.1686477661133));
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task PostalCodeFromCoordinate()
        {
            var postalCode = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "Postcode1");

            Assert.AreEqual("55116", postalCode);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task CoordinateFromAddress()
        {
            var coord = await _service.GetCoordinate("One Microsoft Way", "Redmond", "WA", "98052", "US");

            Assert.IsTrue(coord.Item1.AboutEqual(47.63909));
            Assert.IsTrue(coord.Item2.AboutEqual(-122.1306));
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task CoordinateFromAddressObject()
        {
            var address = new Address()
            {
                addressLine = "One Microsoft Way",
                locality = "Redmond",
                adminDistrict = "WA",
                postalCode = "98052",
                countryRegion = "US"
            };
            var coord = await _service.GetCoordinate(address);

            Assert.IsTrue(coord.Item1.AboutEqual(47.63909));
            Assert.IsTrue(coord.Item2.AboutEqual(-122.1306));
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task CoordinateFromAddressQuery()
        {
            var coord = await _service.QueryCoordinate("One Microsoft Way, Redmond, WA 98052");

            Assert.IsTrue(coord.Item1.AboutEqual(47.63909));
            Assert.IsTrue(coord.Item2.AboutEqual(-122.1306));
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task CoordinateFromLandmark()
        {
            var coord = await _service.GetCoordinate("Eiffel Tower");

            Assert.IsTrue(coord.Item1.AboutEqual(48.858600616455078));
            Assert.IsTrue(coord.Item2.AboutEqual(2.2939798831939697));
        }
    }
}