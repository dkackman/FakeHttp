using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.ServiceLocation;

using UnitTestHelpers;

using MockHttp;

using GalaSoft.MvvmLight.Ioc;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    /// <summary>
    /// Summary description for MockHttpTests
    /// </summary>
    [TestClass]
    public class MockHttpTests
    {
        public MockHttpTests()
        {      
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SimpleIoc.Default.Register<IResponseStore>(() => new FileSystemResponseStore(@"D:\temp\MockResponses"));
            SimpleIoc.Default.Register<HttpClientHandler, MockHttpMessageHandler>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            SimpleIoc.Default.Unregister<HttpClientHandler>();
        }

        [TestMethod]
        [TestCategory("mock-http")]
        public async Task GetSimpleResponseFromMockHttp()
        {                            
            using (dynamic client = new DynamicRestClient("http://openstates.org/api/v1/"))
            {
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                var result = await client.bills.mn("2013s1")("SF 1").get(apikey: key);

                Assert.IsNotNull(result);
                Assert.IsTrue(result.id == "MNB00017167");
            }
        }

        [TestMethod]
        [TestCategory("mock-http")]
        public async Task ExplicitGetInvokeUsingClient()
        {
            using (dynamic client = new DynamicRestClient("http://openstates.org/api/v1/"))
            {
                string key = CredentialStore.RetrieveObject("sunlight.key.json").Key;
                dynamic result = await client.metadata.mn.get(apikey: key);

                Assert.IsNotNull(result);
                Assert.AreEqual("Minnesota", result.name);
            }
        }
    }
}
