using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

using Newtonsoft.Json;

namespace FakeHttp.UnitTests
{
    [TestClass]
    [DeploymentItem(@"FakeResponses\")]
    public class CallbackTest
    {
        class TestCallbacks : ResponseCallbacks
        {
            public override bool FilterParameter(string name, string value)
            {
                return name == "key";
            }

            public async override Task<Stream> Deserialized(ResponseInfo info, Stream content)
            {
                if (info.ResponseHeaders.ContainsKey("Date"))
                {
                    info.ResponseHeaders["Date"] = new List<string>() { DateTimeOffset.UtcNow.ToString("r") };
                }
                return await base.Deserialized(info, content);
            }

            public async override Task<Stream> Serializing(HttpResponseMessage response)
            {
                if (response.RequestMessage.RequestUri.Host == "www.googleapis.com")
                {
                    // get the service content
                    var result = await response.Content.Deserialize<dynamic>();

                    // modify it
                    result.storageClass = "THIS VALUE MASKED";

                    // serialize and return a new stream which will be written to disk
                    var json = JsonConvert.SerializeObject(result);
                    return new MemoryStream(Encoding.UTF8.GetBytes(json));
                }

                return await base.Serializing(response);
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

        [TestMethod]
        [TestCategory("fake")]
        public async Task SetHeaderTimestampViaResponseCallback()
        {
            var handler = new FakeHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, new TestCallbacks()));
            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://dev.virtualearth.net/");
                var response = await client.GetAsync("REST/v1/Locations/?c=en-us&countryregion=us&maxres=1&postalcode=55116");
                response.EnsureSuccessStatusCode();
                Assert.IsTrue(response.Headers.Contains("Date"));

                var stamp = response.Headers.Date;
                Assert.IsTrue(stamp.HasValue);

                var diff = DateTimeOffset.UtcNow - stamp;
                Assert.IsTrue(diff.HasValue);
                Assert.IsTrue(diff.Value.Seconds < 5);  // assert that date stamp to something close to the current time
                                                        // regardless of when it was actually serialzied to disk
            }
        }

        [TestMethod]
        [TestCategory("fake")]
        public async Task MaskContentFieldDuringCapture()
        {
            // capture the response from an enpoiint - TestCallBacks instance will mask a value
            var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\FakeResponses\");
            var capturingHandler = new CapturingHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory, captureFolder, new TestCallbacks()));
            using (var client = new HttpClient(capturingHandler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();
            }

            // now use a fake handler and get the captured response, ensure that value is masked
            var fakeHandler = new FakeHttpMessageHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));
            using (var client = new HttpClient(fakeHandler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                dynamic result = await response.Content.Deserialize<dynamic>();

                Assert.AreEqual("THIS VALUE MASKED", result.storageClass);
            }
        }
    }
}
