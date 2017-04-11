using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

using FakeHttp.Resources;

namespace FakeHttp.UnitTests
{
    [TestClass]
    public class AutomaticTests
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task CanAccessGoogleStorageBucket()
        {
            // this is the path where responses will be stored for future use
            var path = Path.Combine(Path.GetTempPath(), "FakeHttp_UnitTests_");

            var handler = new AutomaticHttpClientHandler(new ResponseStore(new FileSystemResources(path, path)));

            using (var client = new HttpClient(handler, true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                using (var response = await client.GetAsync("storage/v1/b/uspto-pair"))
                {
                    response.EnsureSuccessStatusCode();

                    dynamic metaData = await response.Content.Deserialize<dynamic>();

                    // we got a response and it looks like the one we want
                    Assert.IsNotNull(metaData);
                    Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);
                }
            }
        }

        [TestMethod]
        public async Task ResponseIsStoredWhenNotPresent()
        {
            using (var temp = new TempFolder("FakeHttp_UnitTests"))
            using (var client = new HttpClient(new AutomaticHttpClientHandler(new ResponseStore(new FileSystemResources(temp.RootPath))), true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                using (var response = await client.GetAsync("storage/v1/b/uspto-pair"))
                {
                    response.EnsureSuccessStatusCode();

                    dynamic metaData = await response.Content.Deserialize<dynamic>();

                    // we got a response and it looks like the one we want
                    Assert.IsNotNull(metaData);
                    Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);

                    var responseFolder = Path.Combine(temp.RootPath, @"www.googleapis.com\storage\v1\b\uspto-pair");

                    // assert we that a response file was stored in the expected folder strcutre
                    Assert.IsTrue(Directory.Exists(responseFolder));

                    // assert the response and content were stored in that folder
                    Assert.IsTrue(File.Exists(Path.Combine(responseFolder, "get.response.json")));
                    Assert.IsTrue(File.Exists(Path.Combine(responseFolder, "get.content.json")));
                }
            }
        }

        [TestMethod]
        public async Task StoredResponseIsReturnedWhenPresent()
        {
            using (var temp = new TempFolder("FakeHttp_UnitTests"))
            using (var client = new HttpClient(new AutomaticHttpClientHandler(new ResponseStore(new FileSystemResources(temp.RootPath))), true))
            {
                // create the correct folder and place the response files there
                var responseFolder = Path.Combine(temp.RootPath, @"www.googleapis.com\storage\v1\b\uspto-pair");
                Directory.CreateDirectory(responseFolder);
                Extensions.CopyResourse(responseFolder, "GET.response.json");
                Extensions.CopyResourse(responseFolder, "GET.content.json");

                // now call the http client and make sure we get the response from the file system, not google
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                using (var response = await client.GetAsync("storage/v1/b/uspto-pair"))
                {
                    response.EnsureSuccessStatusCode();

                    dynamic metaData = await response.Content.Deserialize<dynamic>();

                    // we got a response and it looks like the one we want
                    Assert.IsNotNull(metaData);
                    Assert.AreEqual("THIS_IS_THE_FAKE_ONE", metaData.etag); // our embedded resource has this value to differentiate from what google returns
                }
            }
        }
    }
}
