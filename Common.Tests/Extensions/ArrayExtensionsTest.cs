using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Extensions;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class ArrayExtensionsTest
    {
        [TestMethod]
        public void TestAdd()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            var res = arr.Add(6);

            Assert.AreNotSame(arr, res);
            Assert.IsTrue(res.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }));
        }

        [TestMethod]
        public void TestAddToNull()
        {
            int[] arr = null;

            var res = arr.Add(6);

            Assert.AreNotSame(arr, res);
            Assert.IsTrue(res.SequenceEqual(new[] { 6 }));
        }

        [TestMethod]
        public void TestRemove()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            var res = arr.Remove(3);

            Assert.AreNotSame(arr, res);
            Assert.IsTrue(res.SequenceEqual(new[] { 1, 2, 4, 5 }));
        }

        [TestMethod]
        public void TestRemoveNotFound()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            var res = arr.Remove(6);

            Assert.AreSame(arr, res);
        }

        [TestMethod]
        public void TestRemoveFromNull()
        {
            int[] arr = null;

            var res = arr.Remove(6);

            Assert.IsNull(res);
        }

        [TestMethod]
        public void TestReplace()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            var res = arr.Replace(3, 6);

            Assert.AreNotSame(arr, res);
            Assert.IsTrue(res.SequenceEqual(new[] { 1, 2, 6, 4, 5 }));
        }

        [TestMethod]
        public void TestReplaceNotFound()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };

            var res = arr.Replace(6, 9);

            Assert.AreSame(arr, res);
        }

        [TestMethod]
        public void TestReplaceOnNull()
        {
            int[] arr = null;

            var res = arr.Replace(3, 6);

            Assert.IsNull(res);
        }
    }
}
