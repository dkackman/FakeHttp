# Getting Started

## The System Under Test
Because FakeHttp uses a message handler to do irs fakind, the only requirement for the 
SUT is that it not instantiate an HttpMessageHandler itself. This is easily accomplished by taking 
the handler as a parameter, using an IoC containter or via configuration. Whatever mechanism meshes with your design.

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

The FakeHttp library implmenets a handful of different message handler types and a factory class that allow
a suite of tests to easily switch between them. Assuming you already have, or can write, a suite of tests that 
interact with a RESTful service the following steps will allow you to fake them. *(These examples use MSTest but 
the same concepts apply to other test frameworks. There is no dependency on MStest)*

### 1. Replace the HttpMessageHandler Used by the Tests
In order to switch an entire suite of tests from one handler to the next it's easiest to the the MessageHandlerFactory
and its MessageHandlerMode. The mode can be switched in a single place where a test run is initialized.

```csharp
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        // set the http message handler factory to the mode we want for the entire assmebly test execution
        MessageHandlerFactory.Mode = MessageHandlerMode.Fake;
    }
```

### 2. Capture Responses
### 3. Store Responses for Future Use
### 4. Run Tests Against Faked Responses

## Other Considerations

### Sensitive Data
### Storage Mechanisms
### Runtime Control of Faked Responses