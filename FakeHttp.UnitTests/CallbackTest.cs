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
    public class CallbackTest
    {
        class TestCallbacks : ResponseCallbacks
        {
            public override bool FilterParameter(string name, string value)
            {
                return name == "key";
            }
        }

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory("fake")]
        public async Task FilteredQueryParametrIsIgnoredDuringFakingObsoleteMetthod()
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
            var capturingHandler = new CapturingHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder, new TestCallbacks()));
            var fakingHandler = new FakeHttpMessageHandler(new FileSystemResponseStore(captureFolder, new TestCallbacks())); // point the fake to where the capture is stored
            
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
