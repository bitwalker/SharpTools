using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpTools.Extend.Collections;

namespace SharpTools.Test.Extend.Collections
{
    [TestClass]
    public class ArrayExtensionsTest
    {
        [TestMethod]
        public void ArrayExtensions_Slice()
        {
            var nums = Enumerable.Range(0, 100).ToArray();

            var firstTen = nums.Slice(0, 10);
            Assert.IsTrue(Enumerable.Range(0, 10).SequenceEqual(firstTen));

            var lastTen = nums.Slice(90, 10);
            Assert.IsTrue(Enumerable.Range(90, 10).SequenceEqual(lastTen));

            var lowerBoundViolation = nums.Slice(-10, 10);
            Assert.IsTrue(Enumerable.Range(0, 10).SequenceEqual(lowerBoundViolation));

            var upperBoundViolation = nums.Slice(100, 10);
            Assert.IsTrue(new int[0].SequenceEqual(upperBoundViolation));

            var takeAll = nums.Slice(0);
            Assert.IsTrue(nums.SequenceEqual(takeAll));

            var takeAll2 = nums.Slice(0, 1000);
            Assert.IsTrue(nums.SequenceEqual(takeAll2));

            var takeNone = nums.Slice(0, -10);
            Assert.IsTrue(new int[0].SequenceEqual(takeNone));
        }

        [TestMethod]
        public void ArrayExtensions_SliceMap()
        {
            var nums = Enumerable.Range(0, 100).ToArray();

            var firstTen = nums.SliceMap(0, 10, x => x * x);
            var firstTenExpected = Enumerable.Range(0, 10).Select(x => x*x);
            Assert.IsTrue(firstTenExpected.SequenceEqual(firstTen));

            var lastTen = nums.SliceMap(90, 10, x => x * x);
            var lastTenExpected = Enumerable.Range(90, 10).Select(x => x*x);
            Assert.IsTrue(lastTenExpected.SequenceEqual(lastTen));

            var lowerBoundViolation = nums.SliceMap(-10, 10, x => x * x);
            var lowerBoundExpected = Enumerable.Range(0, 10).Select(x => x*x);
            Assert.IsTrue(lowerBoundExpected.SequenceEqual(lowerBoundViolation));

            var upperBoundViolation = nums.SliceMap(100, 10, x => x * x);
            Assert.IsTrue(new int[0].SequenceEqual(upperBoundViolation));

            var takeAll = nums.SliceMap(0, x => x.ToString());
            var takeAllExpected = nums.Select(x => x.ToString());
            Assert.IsTrue(takeAllExpected.SequenceEqual(takeAll));

            var takeAll2 = nums.SliceMap(0, 1000, x => x * x);
            var takeAll2Expected = nums.Select(x => x * x);
            Assert.IsTrue(takeAll2Expected.SequenceEqual(takeAll2));

            var takeNone = nums.SliceMap(0, -10, x => x * x);
            Assert.IsTrue(new int[0].SequenceEqual(takeNone));
        }
    }
}
