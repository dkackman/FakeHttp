# FakeHttp

[Nuget Package](https://www.nuget.org/packages/Dkackman.FakeHttp/)

A library for faking http Rest client communication using System.Net.Http.

Codeproject article with background: http://www.codeproject.com/Articles/1006722/MockHttp 

Allows for writing unit tests to handle http response scenarios from a RESTful service, without needing access to the network or service at test execution time. 
Responses are stored statically on the local file system, in a directory structure that can mapped onto a specific rest endpoint, set of query parameters and http verb.

This allows service layer access code to be unit tested without regard to the actual availability of the real service endpoint. This also ensures that response details and data returned to the test can be controlled, allowing specific test scenarios to be run at any time.

The quickest way to get started is to use an AutomaticHttpClientHandler anywhere you instantiate an System.Net.HttpClient. The automatic handler will first check the local file system for a saved response. If one is not found it will access the live http endpoint, and save the response. Future calls to the same endpoint with identical parameters will return the stored response and content.

    [TestMethod]
    public async Task CanAccessGoogleStorageBucket()
    {
        // this is the path where responses will be stored for future use
        var path = Path.Combine(Path.GetTempPath(), "FakeHttp_UnitTests");

        var handler = new AutomaticHttpClientHandler(new FileSystemResponseStore(path));

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

If more fine grained control is needed over the handler, this workflow can be used without the need to change the service client layer or unit test code:
- Write unit tests that interacts with a live RESTful service that executes the specific client data access functionality under test
- Refine the live tests until they pass
- Execute the test in "capture" mode to record the response of the Rest service
- Execute future test runs in "fake" mode to decouple the tests from the live Rest service

Recorded Http Response messages and content can be edited to create test conditions that simulate Http failure modes or response header values.

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        // the mode controls which type of HttpMessageHandler to create: online, fake or capture
        MessageHandlerFactory.Mode = MessageHandlerMode.Fake;
    }

    [TestMethod]
    [TestCategory("fake")]
    [DeploymentItem(@"FakeResponses\")]
    public async Task CanGetSimpleJsonResult()
    {
        var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\FakeResponses\");
        var handler = MessageHandlerFactory.CreateMessageHandler(TestContext.DeploymentDirectory, captureFolder);

        using (var client = new HttpClient(handler, true))
        {
            client.BaseAddress = new Uri("https://www.googleapis.com/");
            var response = await client.GetAsync("storage/v1/b/uspto-pair");
            response.EnsureSuccessStatusCode();

            dynamic metaData = await response.Deserialize<dynamic>();

            // we got a response and it looks like the one we want
            Assert.IsNotNull(metaData);
            Assert.AreEqual("https://www.googleapis.com/storage/v1/b/uspto-pair", metaData.selfLink);
        }
    }
