using System;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MockHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class ExceptionTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("mock")]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ThrowWhenNoStaticResponseFound()
        {
            var handler = new MockHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");

                var response = await client.GetAsync("metadata/NO_ENDPOINT_HERE");

                response.EnsureSuccessStatusCode();
            }
        }

        [TestMethod]
        [TestCategory("mock")]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ThrowWhenNoStaticResponseFoundByParameterLookup()
        {
            var handler = new MockHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

            using (var mockingClient = new HttpClient(handler, true))
            {
                mockingClient.BaseAddress = new Uri("http://dev.virtualearth.net/");

                var mockedResponse = await mockingClient.GetAsync("REST/v1/Locations?c=en-us&countryregion=us&maxres=1&postalcode=NON_EXISTENT_POSTAL_CODE");

                mockedResponse.EnsureSuccessStatusCode();
            }
        }

        [TestMethod]
        [TestCategory("mock")]
        public async Task ThrowWhenStaticallySimulateFailure()
        {
            var handler = new MockHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");

                var response = await client.PostAsync("metadata/mn", new StringContent("payload"));

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    Assert.IsTrue(e.Message.Contains("401"));
                }
            }
        }
    }
}
