# Getting Started

## The System Under Test

Because FakeHttp uses a message handler to do its faking, the only requirement for the 
SUT is that it not instantiate an [HttpMessageHandler](xref:System.Net.Http.HttpMessageHandler) itself. 
This is easily accomplished by taking the handler as a parameter, using an IoC containter 
or via configuration. Whatever mechanism meshes with your design.

So code that looked like this:

```csharp
public class GeoCoder : IGeoCoder
{
    private readonly HttpClient _httpClient;

    public GeoCoder()
    {
        _httpClient = new HttpClient(new HttpClientHandler(), true);
        _httpClient.BaseAddress = new Uri("http://dev.virtualearth.net/REST/v1/", UriKind.Absolute);
    }
}
```

Might change to this:

```csharp
public class GeoCoder : IGeoCoder
{
    private readonly HttpClient _httpClient;

    public GeoCoder(HttpMessageHandler handler)
    {
        _httpClient = new HttpClient(handler, false); // flag controls disposal of the handler 
        _httpClient.BaseAddress = new Uri("http://dev.virtualearth.net/REST/v1/", UriKind.Absolute);
    }
}
```
## Getting and Using Fake Responses

The easiest way to set up fake response is to capture them from a live service endpoint. Once captured,
they can be stored for future use and/or tweaked to test and simulate how client code behaves with specific response scenarios

FakeHttp implmenets a handful of different message handler types and a factory class that enables
a suite of tests to easily switch between them. Assuming you already have, or can write, a suite of tests that 
interact with a RESTful service the following steps will allow you to fake them. *(These examples use MSTest but 
the same concepts apply to other test frameworks. There is no dependency on MStest)*

### 1. Replace the HttpMessageHandler Used by the Tests

Given a unti test that might look like this, creating the message handler inline:

```csharp
[TestMethod]
public async Task RoundtripPostalCode()
{
    var service = new GeoCoder(new HttpClientHandler(), "api-key");

    var coord = await service.GetCoordinate(new Address() { postalCode = "55116", countryRegion = "US" });
    var address = await service.GetAddress(coord.Item1, coord.Item2);

    Assert.AreEqual("55116", address.postalCode);
}
```

Change it to:

```csharp
[TestMethod]
public async Task RoundtripPostalCode()
{
    var handler = MessageHandlerFactory.CreateMessageHandler(new FileSystemResources(TestContext.TestRunDirectory));
    var service = new GeoCoder(handler, "api-key");

    var coord = await service.GetCoordinate(new Address() { postalCode = "55116", countryRegion = "US" });
    var address = await service.GetAddress(coord.Item1, coord.Item2);

    Assert.AreEqual("55116", address.postalCode);
}
```

With MSTest, [TestContext.TestRunDirectory](https://msdn.microsoft.com/query/dev15.query?appId=Dev15IDEF1&l=EN-US&k=k(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext.TestRunDirectory);k(TargetFrameworkMoniker-.NETFramework,Version%3Dv4.6.1);k(DevLang-csharp)&rd=true) 
is a folder wehre deployed items and test results are created for each test run. It is in this 
folder where the above example will store captured responses.

### 2. Capture Responses

In order to switch an entire suite of tests from one handler to the next it's easiest to use the [MessageHandlerFactory](xref:FakeHttp.MessageHandlerFactory) 
and its [MessageHandlerMode](xref:FakeHttp.MessageHandlerMode). The mode can be switched in a single place where a test run is initialized.

```csharp
[AssemblyInitialize]
public static void AssemblyInitialize(TestContext context)
{
    // set the http message handler factory to the mode we want for the entire assmebly test execution
    MessageHandlerFactory.Mode = MessageHandlerMode.Capture;
}
```

This code tells the factory to create a message handler that will retreive and store the response from any
request it processes.

### 3. Store Responses for Future Use

After running the tests, respsonses and headers will be stored in a folder structure rooted
in the TestContext.TestRunDirectory location (this will be in a folder named *TestResults* in the soltuion
directory). 

![folders](../images/folders.PNG)

There will be a folder named for each host name captured by the unit tests. Copy each of these folders into the
root of your unit test solution diretory or other location for further reference.

Fake data can be made accessible to the unit test in a number of ways:

1. As loose files
1. In a Zip archive
1. As embedded assembly resources

Whichever mechanism is chosen, the directory and file names and structure must be maintained so that FakeHttp
can map http requests to faked responses.

### 4. Run Tests Against Faked Responses

Next we need to wire them into the unit tests and reconfigure FakeHttp to use fakes instead of actual service endpoints.

```csharp
[AssemblyInitialize]
public static void AssemblyInitialize(TestContext context)
{
    // set the http message handler factory to the mode we want for the entire assmebly test execution
    MessageHandlerFactory.Mode = MessageHandlerMode.Fake;
}
```

## Other Considerations

### Sensitive Data

### Storage Mechanisms

### Runtime Control of Faked Responses