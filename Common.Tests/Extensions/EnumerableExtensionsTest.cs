using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Common.Extensions;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class EnumerableExtensionsTest
    {
        [TestMethod]
        public void TestOmitRepeated()
        {
            var sequence = new[] { 1, 2, 2, 3, 3, 3, 1, 2, 3 };

            var result = sequence.OmitRepeated();

            Assert.IsTrue(result.SequenceEqual(new[] { 1, 2, 3, 1, 2, 3 }));
        }

        [TestMethod]
        public void TestOmitRepeatedEmpty()
        {
            var empty = Enumerable.Empty<int>();

            var result = empty.OmitRepeated();

            Assert.IsTrue(result.SequenceEqual(Enumerable.Empty<int>()));
        }

        [TestMethod]
        public void TestOmitRepeatedBy()
        {
            var sequence = new[] { 1, 2, 2, 3, 3, 3, 1, 2, 3 };

            var result = sequence.OmitRepeatedBy(i => i);

            Assert.IsTrue(result.SequenceEqual(new[] { 1, 2, 3, 1, 2, 3 }));
        }

        [TestMethod]
        public void TestOmitRepeatedByEmpty()
        {
            var empty = Enumerable.Empty<int>();

            var result = empty.OmitRepeatedBy(i => i);

            Assert.IsTrue(result.SequenceEqual(Enumerable.Empty<int>()));
        }
    }
}
