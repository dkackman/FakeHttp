# MockHttp
A library for mocking http Rest client communication using System.Net.Http.

Allows for writing unit tests to handle http response scenarios from a RESTful service, without needing access to the network or service at test execution time. Responses are stored statically on the local file system in a directory strucutre that can mapped onto a specific rest endpoint and http verb.

    [TestMethod]
    [TestCategory("mock")]
    public async Task CanGetSimpleJsonResult()
    {
        var handler = new MockHttpClientHandler(new FileSystemResponseStore(TestContext.DeploymentDirectory));

        using (var client = new HttpClient(handler, true))
        {
            client.BaseAddress = new Uri("http://openstates.org/api/v1/");

            var response = await client.GetAsync("metadata/mn");
            response.EnsureSuccessStatusCode();

            dynamic result = await response.Deserialize<dynamic>();

            Assert.IsNotNull(result);
            Assert.AreEqual("Minnesota", result.name);
        }
    }
