using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Extensions;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void TestIsBase64()
        {
            Assert.IsTrue("Zg==".IsBase64());
            Assert.IsTrue("Zm8=".IsBase64());
            Assert.IsTrue("Zm9v".IsBase64());
            Assert.IsTrue("Zm9vYg==".IsBase64());
            Assert.IsTrue("Zm9vYmE=".IsBase64());
            Assert.IsTrue("Zm9vYmFy".IsBase64());
            Assert.IsTrue("Zm9vYmFy+/==".IsBase64());
        }

        [TestMethod]
        public void TestIsNotBase64()
        {
            Assert.IsFalse(StringExtensions.IsBase64(null));
            Assert.IsFalse("".IsBase64());
            Assert.IsFalse("=".IsBase64());
            Assert.IsFalse("Z===".IsBase64());
            Assert.IsFalse("test test test".IsBase64());
            Assert.IsFalse("user:password".IsBase64());
            Assert.IsFalse("user:password===".IsBase64());
            Assert.IsFalse("Connection String;".IsBase64());
        }
    }
}
