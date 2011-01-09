using NoCap.Library.Progress;
using NUnit.Framework;

namespace NoCap.Library.Tests.Util {
    [TestFixture]
    public class MutableProgressTrackerTests {
        [Test]
        public void ZeroProgressOnNew() {
            var mpt = new MutableProgressTracker();

            Assert.AreEqual(0, mpt.Progress);
        }

        [Test]
        public void UpdateRaised() {
            var mpt = new MutableProgressTracker();
            int progressChangedCallCount = 0;

            mpt.ProgressUpdated += (sender, e) => {
                progressChangedCallCount += 1;
            };

            mpt.Progress = 1;
            Assert.AreEqual(1, progressChangedCallCount);

            mpt.Progress = 1;
            Assert.AreEqual(2, progressChangedCallCount);
        }
    }
}
