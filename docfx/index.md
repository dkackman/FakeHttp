# Welcome to **FakeHttp**.
FakeHttp is a .NetStandard library that allows you to "fake" calls to RESTful services.
The advantages of faking http responses in unit tests are:

1. Faking decouples test results from service implementation, resulting in more focused and better tests.
2. Faked responses allow test code to operate on known test cases. This keeps unit test from becoming integration tests.
3. Faking allows client code to be tested under simulated conditions (such as service unavailble) without the need to modify the service.
4. Faking can dramatically improve the performance of http client tests by removing internet latency.

## How does FakeHttp work?
FakeHttp works by deriving from [HttpMessageHandler](xref:System.Net.Http.HttpMessageHandler). It can capture and store actual service 
responses and cache them locally as JSON files (headers and content). These can then be tweaked, commited to source control 
and used for subsequent unt testing.

## Where to from here?
- Get the [Nuget Pacakge](https://www.nuget.org/packages/Dkackman.FakeHttp/)
- [Getting Started](articles/GettingStarted.md)
- [API Documentation](api/index.md)
- Advanced Topics