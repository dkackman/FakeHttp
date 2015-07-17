using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using FakeHttp.Desktop;

using Newtonsoft.Json;

namespace FakeHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"FakeResponses\")]
    public class ExampleTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task MinimalExampleTest()
        {
            var handler = new FakeHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));
            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.example.com/");
                var response = await client.GetAsync("HelloWorldService");
                response.EnsureSuccessStatusCode();

                dynamic content = await response.Content.Deserialize<dynamic>();

                Assert.IsNotNull(content);
                Assert.AreEqual("Hello World", content.Message);
            }
        }
    }
}
