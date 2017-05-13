using System.IO;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.ServiceLocation;

using GalaSoft.MvvmLight.Ioc;

using FakeHttp;
using FakeHttp.Resources;

namespace BingGeoCoder.Client.UnitTests
{
    [TestClass]
    public class AssemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // set the http message handler factory to the mode we want for the entire assmebly test execution
            MessageHandlerFactory.Mode = MessageHandlerMode.Fake;

            // setup IOC so test classes can get the shared message handler
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // responses are in a ziparchive that is also referenced by the DeploymentItem attributes
            var archivePath = Path.Combine(context.DeploymentDirectory, "FakeResponses.zip");
            SimpleIoc.Default.Register(() => MessageHandlerFactory.CreateMessageHandler(new ZipResources(archivePath)));
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (SimpleIoc.Default.IsRegistered<HttpMessageHandler>())
            {
                SimpleIoc.Default.GetInstance<HttpMessageHandler>().Dispose();
            }
        }
    }
}
