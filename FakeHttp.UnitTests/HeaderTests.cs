using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

using FakeHttp.Desktop;

using Newtonsoft.Json;

namespace FakeHttp.UnitTests
{
    [TestClass]
    public class HeaderTests
    {
        [TestMethod]
        public async Task ContentHeaderAreRestored()
        {
            using (var temp = new TempFolder("FakeHttp_UnitTests"))
            using (var client = new HttpClient(new AutomaticHttpClientHandler(new FileSystemResponseStore(temp.RootPath, temp.RootPath)), true))
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

                    Assert.IsTrue(response.Content.Headers.Contains("Fake"));
                }
            }
        }
    }
}
