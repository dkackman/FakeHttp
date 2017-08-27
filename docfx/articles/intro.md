# Introduction

When writing unit test code it is important to isolate the code under test, to the greatest extent possible, 
in order to ensure that the tests are atomic. The more things a single unit test is dependant, on the higher 
the liklihood that success or failure will become unrelated to the intent of the test.
When writing the [Dynamic Rest Client](https://github.com/dkackman/DynamicRestProxy) for example, 
I created some unit tests that used Bing's Locations Rest API. These tests would fail on occasion in such a way 
that when they failed, if I ran them immediately afterwards, they would succeed. This was very difficult to figure 
out and it was ultimately caused by how the Bing service responded when the service was very busy and had nothing 
to do with my rest client at all. This ultimately lead me to create this library so that I could fake the service 
response to ensure that what was being unit tested was the code under test and nothing more.

Now, creating http clients is not something that we do every day but writing client side rest service 
wrappers is pretty common. This library should come in handy for unit testing any code that uses the 
System.Net.Http.HttpClient component. It is designed to minimally intrusive to existing code
making it relatively easy to fake http traffic without major changes to already written components 
or tests.