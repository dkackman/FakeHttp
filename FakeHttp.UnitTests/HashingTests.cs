using System;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeHttp.UnitTests
{
    [TestClass]
    public class HashingTests
    {
        [TestMethod]
        public void HashIsOrderIndependant()
        {
            var formatter = new Desktop.DesktopMessageFormatter();

            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?a=1&b=2&c=3");
            var fileName1 = formatter.ToFileName(request1);

            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?c=3&a=1&b=2");
            var fileName2 = formatter.ToFileName(request2);

            Assert.AreEqual(fileName1, fileName2);
        }

        [TestMethod]
        public void HashIsCaseInsensitive()
        {
            var formatter = new Desktop.DesktopMessageFormatter();

            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?hi=there");
            var fileName1 = formatter.ToFileName(request1);

            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?HI=THERE");
            var fileName2 = formatter.ToFileName(request2);

            Assert.AreEqual(fileName1, fileName2);
        }
    }
}
