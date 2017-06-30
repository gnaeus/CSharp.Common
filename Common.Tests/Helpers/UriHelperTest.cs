using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Helpers;

namespace Common.Tests.Helpers
{
    [TestClass]
    public class UriHelperTest
    {
        [TestMethod]
        public void AddTrailingSlashShouldIgnoreEmptyUrls()
        {
            Assert.IsNull(UriHelper.AddTrailingSlash(null));
            Assert.AreEqual("", UriHelper.AddTrailingSlash(""));
            Assert.AreEqual(" \t", UriHelper.AddTrailingSlash(" \t"));
        }

        [TestMethod]
        public void AddTrailingSlashShouldWorkWithAbsoluteUrl()
        {
            Assert.AreEqual("http://microsoft.com/", UriHelper.AddTrailingSlash("http://microsoft.com"));
            Assert.AreEqual("http://microsoft.com/", UriHelper.AddTrailingSlash("http://microsoft.com/"));
        }

        [TestMethod]
        public void AddTrailingSlashShouldWorkWithRelativeUrl()
        {
            Assert.AreEqual("/reports/", UriHelper.AddTrailingSlash("/reports"));
            Assert.AreEqual("/reports/", UriHelper.AddTrailingSlash("/reports/"));
            Assert.AreEqual("/", UriHelper.AddTrailingSlash("/"));
        }
    }
}
