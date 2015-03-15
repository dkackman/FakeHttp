using System;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MockHttp;

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
            var handler = new MockHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

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
        public async Task ThrowWhenStaticallySimulateFailure()
        {
            var handler = new MockHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");

                var response = await client.PostAsync("metadata/mn", new StringContent("payload"));

                response.EnsureSuccessStatusCode();
            }
        }
    }
}
