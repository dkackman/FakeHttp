using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public async Task ResponseIsStoredWhenNotPresent()
        {
            var captureFolder = Path.Combine(Path.GetTempPath(), "FakeHttp_UnitTests");

            var handler = new AutomaticHttpClientHandler(new FileSystemResponseStore(captureFolder, captureFolder));

            var responseFolder = Path.Combine(captureFolder, @"www.googleapis.com\storage\v1\b\uspto-pair");

            // make sure the target folder doesn't exist when we start
            if (Directory.Exists(captureFolder))
            {
                Directory.Delete(captureFolder, true);
            }

            try
            {
                using (var client = new HttpClient(handler, true))
                {
                    client.BaseAddress = new Uri("https://www.googleapis.com/");
                    var response = await client.GetAsync("storage/v1/b/uspto-pair");
                    response.EnsureSuccessStatusCode();

                    dynamic metaData = await response.Content.Deserialize<dynamic>();

                    // we got a response and it looks like the one we want
                    Assert.IsNotNull(metaData);
                    Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);

                    // assert we that a response file was stored in teh expected folder strcutre
                    Assert.IsTrue(Directory.Exists(responseFolder));

                    // assert the response and content were stored in that folder
                    Assert.IsTrue(File.Exists(Path.Combine(responseFolder, "get.response.json")));
                    Assert.IsTrue(File.Exists(Path.Combine(responseFolder, "get.content.json")));
                }
            }
            finally
            {
                // remove our temp folders
                Directory.Delete(captureFolder, true);
            }
        }
    }
}
