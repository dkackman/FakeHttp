# Welcome to **FakeHttp**.
FakeHttp is a .NetStandard library that allows you to "fake" calls to RESTful services.
The advantages of faking http responses in unit tests are:

1. Faking decouples test results from service implementation. Faked responses allow test code to operate on known test cases. This keeps unit test from becoming integration tests.
2. Faking allows client code to be tested under simulated conditions (such as service unavailble) without the need to modify the service.
3. Faking can dramatically improve the performance of http client tests by removing internet latency.

## How does FakeHttp work?
FakeHttp works by deriving from System.Net.Http.HttpMessageHandler. It can capture and store actual service 
responses and cache them locally as JSON files (headers and content). These can then be tweaked, commited to source control with the tests
and used 

## Quick Start Notes:
1. Add images to the *images* folder if the file is referencing an image.
