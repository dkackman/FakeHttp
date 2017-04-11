using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using FakeHttp.Resources;

namespace FakeHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"FakeResponses\")]
    public class CaptureTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("fake")]
        public async Task CaptureResponse()
        {
            // store the rest response in a subfolder of the solution directory for future use
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\FakeResponses\");
            var handler = new CapturingHttpClientHandler(new ResponseStore(new FileSystemResources(captureFolder)));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                dynamic metaData = await response.Content.Deserialize<dynamic>();

                // we got a response and it looks like the one we want
                Assert.IsNotNull(metaData);
                Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);

                // assert we stored it where we want it to go
                var formatter = new MessageFormatter();
                var folderPath = Path.Combine(captureFolder, formatter.ToResourcePath(response.RequestMessage.RequestUri), response.RequestMessage.Method.Method);
                Assert.IsTrue(File.Exists(folderPath + ".response.json"));
                Assert.IsTrue(File.Exists(folderPath + ".content.json"));
            }
        }

        [TestMethod]
        [TestCategory("fake")]
        public async Task CaptureAndFakeResponsesMatch()
        {
            // store the rest response in a subfolder of the solution directory for future use
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\FakeResponses\");

            var capturingHandler = new CapturingHttpClientHandler(new ResponseStore(new FileSystemResources(captureFolder)));
            var fakingHandler = new FakeHttpMessageHandler(new ReadOnlyResponseStore(new FileSystemResources(captureFolder))); // point the fake to where the capture is stored

            using (var capturingClient = new HttpClient(capturingHandler, true))
            using (var fakingClient = new HttpClient(fakingHandler, true))
            {
                capturingClient.BaseAddress = new Uri("https://www.googleapis.com/");
                fakingClient.BaseAddress = new Uri("https://www.googleapis.com/");

                using (var capturedResponse = await capturingClient.GetAsync("storage/v1/b/uspto-pair"))
                using (var fakedResponse = await fakingClient.GetAsync("storage/v1/b/uspto-pair"))
                {
                    capturedResponse.EnsureSuccessStatusCode();
                    fakedResponse.EnsureSuccessStatusCode();

                    string captured = await capturedResponse.Content.Deserialize<string>();
                    string faked = await fakedResponse.Content.Deserialize<string>();

                    Assert.AreEqual(captured, faked);
                }
            }
        }
    }
}