using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

using FakeHttp.Desktop;

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
        [TestCategory("fake")]
        public async Task CaptureAndFakeResponsesMatch()
        {
            // store the rest response in a subfolder of the solution directory for future use
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\FakeResponses\");

            var capturingHandler = new CapturingHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder));
            var fakingHandler = new FakeHttpMessageHandler(new FileSystemResponseStore(captureFolder)); // point the fake to where the capture is stored

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

        [TestMethod]
        [TestCategory("fake")]
        public async Task FilteredQueryParametrIsIgnoredDuringFaking()
        {
            string key = CredentialStore.RetrieveObject("bing.key.json").Key;
            // store the rest response in a subfolder of the solution directory for future use
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\FakeResponses\");

            // when capturing the real response, we do not want to serialize things like api keys
            // both because that is a possible infomration leak and also because it would
            // bind the serialized resposne to the key, making successful faking dependent on
            // the key used when capturing the response. The fake response lookup will try to find
            // a serialized response that matches a hash of all the query paramerters. The lambda in
            // the response store constructor below allows us to ignore certain paramters for that lookup
            // when capturing and faking responses
            //
            // this test ensures that our mechansim to filter out those paramters we want to ignore works
            //
            var capturingHandler = new CapturingHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder, (name, value) => name == "key"));
            var fakingHandler = new FakeHttpMessageHandler(new FileSystemResponseStore(captureFolder, (name, value) => name == "key")); // point the fake to where the capture is stored

            using (var capturingClient = new HttpClient(capturingHandler, true))
            using (var fakingClient = new HttpClient(fakingHandler, true))
            {
                capturingClient.BaseAddress = new Uri("http://dev.virtualearth.net/");
                fakingClient.BaseAddress = new Uri("http://dev.virtualearth.net/");

                using (var capturedResponse = await capturingClient.GetAsync("REST/v1/Locations?c=en-us&countryregion=us&maxres=1&postalcode=55116&key=" + key))
                using (var fakedResponse = await fakingClient.GetAsync("REST/v1/Locations?c=en-us&countryregion=us&maxres=1&postalcode=55116&key=THIS_SHOULD_NOT_MATTER"))
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