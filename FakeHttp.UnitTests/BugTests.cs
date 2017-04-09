using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using FakeHttp.FileSystem;

namespace FakeHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"FakeResponses\")]
    public class BugTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task FakeResponseHadRequestMessageSet()
        {
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\FakeResponses\");
            var handler = new FakeHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                using (var response = await client.GetAsync("storage/v1/b/uspto-pair"))
                {
                    response.EnsureSuccessStatusCode();
                    Assert.IsNotNull(response.RequestMessage);
                }
            }
        }
    }
}