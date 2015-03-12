using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace MockHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class CaptureTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task CaptureResponse()
        {
            var handler = new MockHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, Path.Combine(TestContext.TestRunDirectory, @"..\..\MockResponses\")));            

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                dynamic metaData = await response.Deserialize<dynamic>(new JsonSerializerSettings());
                Assert.IsNotNull(metaData);
                Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);
            }
        }
    }
}
