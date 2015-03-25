# MockHttp
A library for mocking http Rest client communication using System.Net.Http.

Allows for writing unit tests to handle http response scenarios from a RESTful service, without needing access to the network or service at test execution time. 
Responses are stored statically on the local file system in a directory strucutre that can mapped onto a specific rest endpoint, set of query parameters and http verb.

This allows service data access layer code to be unit tested without regard to the actual availability of the tart service. This also ensures that response details and data returned to the test can be controlled.

It supports this basic workflow without the need to change the service client layer or unit test code:
- Write unit tests that interacts with a live RESTful service that executes the specific client data access functionality under test
- Refine the tests until they pass
- Execute the test in "capture" mode to record the response of the service
- Execute future test runs in "mock" mode to decoulpe the tests from the live Rest srvice

Recorded Http Response messages and content can be edited to create test conditions that simulate Http failure modes or response header values.

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        // the mode controls which type of HttpMessageHandler to create: online, mock or capture
        MessageHandlerFactory.Mode = MessageHandlerMode.Mock;
    }

    [TestMethod]
    [TestCategory("mock")]
    [DeploymentItem(@"MockResponses\")]
    public async Task CanGetSimpleJsonResult()
    {
        var captureFolder = Path.Combine(TestContext.TestRunDirectory, @"..\..\MockResponses\");
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
