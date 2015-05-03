using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

using MockHttp.Desktop;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MockHttp.UnitTests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public async Task VerifyResponseSerializationFormat()
        {            
            using (var client = new HttpClient(new HttpClientHandler(), true))
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                var response = await client.GetAsync("storage/v1/b/uspto-pair");
                response.EnsureSuccessStatusCode();

                // this is the object that is serialized (response, normalized request query and pointer to the content file)
                var info = new ResponseInfo()
                {
                    Response = response,
                    Query = "doesn't matter",
                    ContentFileName = "doesn't matter"
                };

                var json = JsonConvert.SerializeObject(info, Formatting.Indented, new HttpResponseMessageConverter());

                //mostly we want to make sure serialzie doesn't except
                Assert.IsFalse(string.IsNullOrEmpty(json));
            }
        }
    }
}
