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
            var formatter = new MessageFormatter();

            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?a=1&b=2&c=3");
            var fileName1 = formatter.ToName(request1, (s1,s2) => false);

            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?c=3&a=1&b=2");
            var fileName2 = formatter.ToName(request2, (s1, s2) => false);

            Assert.AreEqual(fileName1, fileName2);
        }

        [TestMethod]
        public void HashIsCaseInsensitive()
        {
            var formatter = new MessageFormatter();

            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?hi=there");
            var fileName1 = formatter.ToName(request1, (s1, s2) => false);

            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://www.example.com?HI=THERE");
            var fileName2 = formatter.ToName(request2, (s1, s2) => false);

            Assert.AreEqual(fileName1, fileName2);
        }
    }
}
