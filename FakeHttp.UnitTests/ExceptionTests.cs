using System;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"FakeResponses\")]
    public class ExceptionTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("fake")]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ThrowWhenNoStaticResponseFound()
        {
            var handler = new FakeHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");

                var response = await client.GetAsync("metadata/NO_ENDPOINT_HERE");

                response.EnsureSuccessStatusCode();
            }
        }

        [TestMethod]
        [TestCategory("fake")]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ThrowWhenNoStaticResponseFoundByParameterLookup()
        {
            var handler = new FakeHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

            using (var fakingClient = new HttpClient(handler, true))
            {
                fakingClient.BaseAddress = new Uri("http://dev.virtualearth.net/");

                var fakedResponse = await fakingClient.GetAsync("REST/v1/Locations?c=en-us&countryregion=us&maxres=1&postalcode=NON_EXISTENT_POSTAL_CODE");

                fakedResponse.EnsureSuccessStatusCode();
            }
        }

        [TestMethod]
        [TestCategory("fake")]
        public async Task ThrowWhenStaticallySimulateFailure()
        {
            var handler = new FakeHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

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
