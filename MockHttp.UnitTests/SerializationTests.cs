using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MockHttp.Desktop;

using Newtonsoft.Json;

namespace MockHttp.UnitTests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        [TestCategory("mock")]
        public async Task ResponseInfoPackedCorrectly()
        {            
            using (var client = new HttpClient(new HttpClientHandler(), true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                var formatter = new DesktopRequestFormatter();

                // this is the object that is serialized (response, normalized request query and pointer to the content file)
                var info = formatter.PackageResponse(response);

                Assert.IsNotNull(info);
                Assert.IsTrue(info.ContentFileName.EndsWith("json"));
            }
        }

        [TestMethod]
        [TestCategory("mock")]
        public async Task RoundTripResponseInfo()
        {
            using (var client = new HttpClient(new HttpClientHandler(), true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                var formatter = new DesktopRequestFormatter();

                // this is the object that is serialized (response, normalized request query and pointer to the content file)
                var info = formatter.PackageResponse(response);

                var json = JsonConvert.SerializeObject(info, Formatting.Indented);
                var newInfo = JsonConvert.DeserializeObject<ResponseInfo>(json);

                Assert.AreEqual(info, newInfo);
            }
        }

        [TestMethod]
        [TestCategory("mock")]
        public async Task CreateContentFromSerializedResponse()
        {
            using (var client = new HttpClient(new HttpClientHandler(), true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                var formatter = new DesktopRequestFormatter();

                // this is the object that is serialized (response, normalized request query and pointer to the content file)
                var info = formatter.PackageResponse(response);
                var json = JsonConvert.SerializeObject(info, Formatting.Indented);

                var newInfo = JsonConvert.DeserializeObject<ResponseInfo>(json);
                var content = newInfo.CreateContent(new MemoryStream());

                Assert.AreEqual("UTF-8", content.Headers.ContentType.CharSet);
                Assert.AreEqual("application/json", content.Headers.ContentType.MediaType);
            }
        }
    }
}
