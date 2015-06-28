using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

using MockHttp.Desktop;

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
        [TestCategory("mock")]
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

                dynamic metaData = await response.Content.Deserialize<dynamic>();

                // we got a response and it looks like the one we want
                Assert.IsNotNull(metaData);
                Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);

                // assert we stored it where we want it to go
                var formatter = new DesktopMessagetFormatter();
                var folderPath = Path.Combine(captureFolder, formatter.ToFolderPath(response.RequestMessage.RequestUri), response.RequestMessage.Method.Method);
                Assert.IsTrue(File.Exists(folderPath + ".response.json"));
                Assert.IsTrue(File.Exists(folderPath + ".content.json"));
            }
        }

        [TestMethod]
        [TestCategory("mock")]
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

                string captured = await capturedResponse.Content.Deserialize<string>();
                string mocked = await mockedResponse.Content.Deserialize<string>();

                Assert.AreEqual(captured, mocked);
            }
        }

        [TestMethod]
        [TestCategory("mock")]
        public async Task FilteredQueryParametrIsIgnoredDuringMocking()
        {
            string key = CredentialStore.RetrieveObject("bing.key.json").Key;
            // store the rest response in a subfolder of the solution directory for future use
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\MockResponses\");

            // when capturing the real response, we do not want to serialize things like api keys
            // both because that is a possible infomration leak and also because it would
            // bind the serialized resposne to the key, making successful mocking dependent on
            // the key used when capturing the response. The mock response lookup will try to find
            // a serialized response that matches a hash of all the query paramerters. The lambda in
            // the response store constructor below allows us to ignore certain paramters for that lookup
            // when capturing and mocking responses
            //
            // this test ensures that our mechansim to filter out those paramters we want to ignore works
            //
            var capturingHandler = new CapturingHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder, (name, value) => name == "key"));
            var mockingHandler = new MockHttpMessageHandler(new FileSystemResponseStore(captureFolder, (name, value) => name == "key")); // point the mock to where the capture is stored

            using (var capturingClient = new HttpClient(capturingHandler, true))
            using (var mockingClient = new HttpClient(mockingHandler, true))
            {
                capturingClient.BaseAddress = new Uri("http://dev.virtualearth.net/");
                mockingClient.BaseAddress = new Uri("http://dev.virtualearth.net/");

                var capturedResponse = await capturingClient.GetAsync("REST/v1/Locations?c=en-us&countryregion=us&maxres=1&postalcode=55116&key=" + key);
                var mockedResponse = await mockingClient.GetAsync("REST/v1/Locations?c=en-us&countryregion=us&maxres=1&postalcode=55116&key=THIS_SHOULD_NOT_MATTER");

                capturedResponse.EnsureSuccessStatusCode();
                mockedResponse.EnsureSuccessStatusCode();

                string captured = await capturedResponse.Content.Deserialize<string>();
                string mocked = await mockedResponse.Content.Deserialize<string>();

                Assert.AreEqual(captured, mocked);
            }
        }
    }
}
