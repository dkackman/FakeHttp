using System.Threading.Tasks;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using BingGeoCoder.Client;

using Locations.ViewModels;

using FakeHttp;
using FakeHttp.Resources;

namespace ViewModelTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task NotFoundSetsErrorMessage()
        {
            var handler = new FakeHttpMessageHandler(new AssemblyResources(this.GetType().GetTypeInfo().Assembly));
            var geocoder = new GeoCoder(handler, "key");

            var vm = new MainViewModel(geocoder)
            {
                Landmark = "This won't be found"
            };
            await vm.Lookup();

            Assert.IsTrue(!string.IsNullOrEmpty(vm.ErrorMessage));
            Assert.IsTrue(vm.ErrorMessage.Contains("404"));
        }

        [TestMethod]
        public async Task LookupSetsLocation()
        {
            var handler = new FakeHttpMessageHandler(new AssemblyResources(this.GetType().GetTypeInfo().Assembly));
            var geocoder = new GeoCoder(handler, "xxxxx");

            var vm = new MainViewModel(geocoder)
            {
                Landmark = "Eiffel Tower"
            };
            await vm.Lookup();

            Assert.IsTrue(!string.IsNullOrEmpty(vm.Location));
        }
    }
}
