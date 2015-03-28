using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.ServiceLocation;

using GalaSoft.MvvmLight.Ioc;

using BingGeoCoder.Client;

using UnitTestHelpers;

using MockHttp;

namespace GeoCoderTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class AddressPartTests
    {
        private static IGeoCoder _service;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // set the http message handler factory to the mode we want for the entire assmebly test execution
            MessageHandlerFactory.Mode = MessageHandlerMode.Mock;

            // setup IOC so test classes can get the shared message handler
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // folders where mock responses are stored and where captured response should be saved
            var mockFolder = context.DeploymentDirectory; // the folder where the unit tests are running
            var captureFolder = Path.Combine(context.TestRunDirectory, @"..\..\MockResponses\"); // kinda hacky but this should be the solution folder

            // here we don't want to serialize or include our api key in response lookups so
            // pass a lambda that will indicate to the serialzier to filter that param out
            SimpleIoc.Default.Register<HttpMessageHandler>(() =>
                MessageHandlerFactory.CreateMessageHandler(mockFolder, captureFolder, (name, value) => name.Equals("key", StringComparison.InvariantCultureIgnoreCase)));
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (SimpleIoc.Default.IsRegistered<HttpMessageHandler>())
            {
                SimpleIoc.Default.GetInstance<HttpMessageHandler>().Dispose();
            }
        }

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
        public async Task GetFormattedAddressFromCoordinate()
        {
            var address = await _service.GetFormattedAddress(44.9108238220215, -93.1702041625977);

            Assert.AreEqual("1012 Davern St, St Paul, MN 55116", address);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetNeighborhoodFromCoordinate()
        {
            var address = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "Neighborhood");

            Assert.AreEqual("Highland", address);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetAddressFromCoordinate()
        {
            var address = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "Address");

            Assert.AreEqual("1012 Davern St", address);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetPostalCodeFromCoordinate()
        {
            var postalCode = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "Postcode1");

            Assert.AreEqual("55116", postalCode);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCityFromCoordinate()
        {
            var city = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "PopulatedPlace");

            Assert.AreEqual("St Paul", city);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCountyFromCoordinate()
        {
            var county = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "AdminDivision2");

            Assert.AreEqual("Ramsey Co.", county);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetStateFromCoordinate()
        {
            var state = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "AdminDivision1");

            Assert.AreEqual("MN", state);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCountryFromCoordinate()
        {
            var country = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, "CountryRegion");

            Assert.AreEqual("United States", country);
        }

        [TestMethod]
        [TestCategory("geocoder")]
        public async Task GetCountryFromCoordinateUsingEnum()
        {
            var country = await _service.GetAddressPart(44.9108238220215, -93.1702041625977, AddressEntityType.CountryRegion);

            Assert.AreEqual("United States", country);
        }
    }
}
