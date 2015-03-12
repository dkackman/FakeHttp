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
            // store the rest response in a subfolder of the solution directory for future use
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\MockResponses\");
            var handler = new CapturingHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder));            

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                dynamic metaData = await response.Deserialize<dynamic>(new JsonSerializerSettings());

                // we got a response and it looks like the one we want
                Assert.IsNotNull(metaData);
                Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);

                // assert we stored it where we want it to go
                var responseFullPath = Path.Combine(captureFolder, response.RequestMessage.RequestUri.ToFilePath(), response.RequestMessage.Method + ".json");
                Assert.IsTrue(File.Exists(responseFullPath));
            }
        }
    }
}
