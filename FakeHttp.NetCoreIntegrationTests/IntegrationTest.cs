using FakeHttp.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FakeHttp.NetCoreIntegrationTests
{
    [TestClass]
    public class IntegrationTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task FullRequestCycleTests()
        {
            MessageHandlerFactory.Mode = MessageHandlerMode.Capture;
            
            var handler = MessageHandlerFactory.CreateMessageHandler(new FileSystemResources(Path.GetTempPath()));
            using (var client = new HttpClient(handler, true))
            {
                var response = await client.GetAsync("https://www.example.com");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                Assert.IsNotNull(content);
                Assert.IsTrue(content.Contains("This domain is established to be used for illustrative examples in documents."));
            }


            MessageHandlerFactory.Mode = MessageHandlerMode.Fake;

            var fakeHandler = MessageHandlerFactory.CreateMessageHandler(new FileSystemResources(Path.GetTempPath()));
            using (var client = new HttpClient(fakeHandler, true))
            {
                var response = await client.GetAsync("https://www.example.com");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                Assert.IsNotNull(content);
                Assert.IsTrue(content.Contains("This domain is established to be used for illustrative examples in documents."));
            }
        }
    }
}
