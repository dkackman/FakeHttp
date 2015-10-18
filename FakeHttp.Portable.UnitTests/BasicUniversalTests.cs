using System;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.ApplicationModel;

namespace FakeHttp.Portable.UnitTests
{
    [TestClass]
    public class BasicUniversalTests
    {
        [TestMethod]
        [TestCategory("Universal")]
        public async Task PortableQueryHash()
        {
            var responses = await Package.Current.InstalledLocation.GetFolderAsync("FakeResponses");

            using (var fakeHandler = new FakeHttpMessageHandler(new StorageFolderResponseStore(responses)))
            using (var client = new HttpClient(fakeHandler, false))
            {
                client.BaseAddress = new Uri("http://dev.virtualearth.net/");
                using (var response = await client.GetAsync("REST/v1/Locations?c=en-us&countryregion=us&maxres=1&postalcode=55116"))
                {
                    response.EnsureSuccessStatusCode();

                    dynamic content = await response.Content.Deserialize<dynamic>();

                    Assert.IsNotNull(content);
                    Assert.AreEqual("OK", content.statusDescription);
                }
            }
        }

        [TestMethod]
        [TestCategory("Universal")]
        public async Task MinimalExampleTest()
        {
            var responses = await Package.Current.InstalledLocation.GetFolderAsync("FakeResponses");

            using (var fakeHandler = new FakeHttpMessageHandler(new StorageFolderResponseStore(responses)))
            using (var client = new HttpClient(fakeHandler, false))
            {
                client.BaseAddress = new Uri("https://www.example.com/");
                using (var response = await client.GetAsync("HelloWorldService"))
                {
                    response.EnsureSuccessStatusCode();

                    dynamic content = await response.Content.Deserialize<dynamic>();

                    Assert.IsNotNull(content);
                    Assert.AreEqual("Hello World", content.Message);
                }
            }
        }

        [TestMethod]
        [TestCategory("Universal")]
        public async Task GetPublicBucket()
        {
            var responses = await Package.Current.InstalledLocation.GetFolderAsync("FakeResponses");

            using (var fakeHandler = new FakeHttpMessageHandler(new StorageFolderResponseStore(responses)))
            using (var client = new HttpClient(fakeHandler, false))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                using (var response = await client.GetAsync("storage/v1/b/uspto-pair"))
                {
                    Assert.IsNotNull(response);
                    response.EnsureSuccessStatusCode();

                    dynamic result = await response.Content.Deserialize<dynamic>();
                    Assert.IsNotNull(result);
                    Assert.AreEqual("uspto-pair", result.name);
                }
            }
        }
    }
}