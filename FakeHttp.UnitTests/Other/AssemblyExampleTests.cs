using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using FakeHttp.Resources;

namespace FakeHttp.UnitTests.Other
{
    [TestClass]
    public class AssemblyExampleTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("Embedded resource")]
        public async Task CanRetreiveEmbeedResourceResponse()
        {
            var resources = new AssemblyResources(Assembly.GetExecutingAssembly());
            var handler = new FakeHttpMessageHandler(resources);
            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");

                using (var response = await client.GetAsync("storage/v1/b/uspto-pair"))
                {
                    response.EnsureSuccessStatusCode();

                    dynamic metaData = await response.Content.Deserialize<dynamic>();

                    // we got a response and it looks like the one we want
                    Assert.IsNotNull(metaData);
                    Assert.AreEqual("THIS_IS_THE_FAKE_ONE", metaData.etag); // our embedded resource has this value to differentiate from what google returns
                }
            }
        }
    }
}
