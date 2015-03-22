using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using MockHttp;
using UnitTestHelpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MockHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class FactoryBasedHandlerTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("mock")]
        public async Task CanGetSimpleJsonResult()
        {
            var handler = MessageHandlerFactory.CreateMessageHandler(TestContext.DeploymentDirectory, Path.Combine(TestContext.TestRunDirectory, @"..\..\MockResponses\"));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("http://openstates.org/api/v1/");
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                client.DefaultRequestHeaders.Add("X-APIKEY", key);

                var response = await client.GetAsync("metadata/mn");
                response.EnsureSuccessStatusCode();

                dynamic result = await response.Deserialize<dynamic>();

                Assert.IsNotNull(result);
                Assert.AreEqual("Minnesota", result.name);
            }
        }
    }
}
