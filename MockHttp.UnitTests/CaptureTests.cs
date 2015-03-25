using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

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
        [TestCategory("capture")]
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

                dynamic metaData = await response.Deserialize<dynamic>();

                // we got a response and it looks like the one we want
                Assert.IsNotNull(metaData);
                Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);

                // assert we stored it where we want it to go
                var folderPath = Path.Combine(captureFolder, response.RequestMessage.RequestUri.ToFilePath(), response.RequestMessage.Method.Method);
                Assert.IsTrue(File.Exists(folderPath + ".response.json"));
                Assert.IsTrue(File.Exists(folderPath + ".content.json"));
            }
        }

        [TestMethod]
        public async Task CaptureAndMockResponsesMatch()
        {
            // store the rest response in a subfolder of the solution directory for future use
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\MockResponses\");

            var capturingHandler = new CapturingHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder));
            var mockingHandler = new MockHttpMessageHandler(new FileSystemResponseStore(captureFolder)); // point the mock to where the capture is stored

            using (var capturingClient = new HttpClient(capturingHandler, true))
            using (var mockingClient = new HttpClient(mockingHandler, true))
            {
                capturingClient.BaseAddress = new Uri("https://www.googleapis.com/");
                mockingClient.BaseAddress = new Uri("https://www.googleapis.com/");

                var capturedResponse = await capturingClient.GetAsync("storage/v1/b/uspto-pair");
                var mockedResponse = await mockingClient.GetAsync("storage/v1/b/uspto-pair");

                capturedResponse.EnsureSuccessStatusCode();
                mockedResponse.EnsureSuccessStatusCode();

                dynamic captured = await capturedResponse.Deserialize<dynamic>();
                dynamic mocked = await mockedResponse.Deserialize<dynamic>();

                var comparer = new DictionaryComparer<string, object>();
                Assert.IsTrue(comparer.Equals(captured, mocked));
            }
        }
    }
}
