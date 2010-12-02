using System;
using NoCap.Library.Progress;
using NUnit.Framework;

namespace NoCap.Library.Tests.Util {
    [TestFixture]
    public class NotifyingProgressTrackerTests {
        [Test]
        public void ZeroProgressOnNew() {
            var npt = new NotifyingProgressTracker();

            Assert.AreEqual(0, npt.Progress);
        }

        [Test]
        public void NotifyRaised() {
            var npt = new NotifyingProgressTracker();
            int progressChangedCallCount = 0;

            npt.ProgressUpdated += (sender, e) => {
                progressChangedCallCount += 1;
            };

            npt.Progress = 1;
            Assert.AreEqual(1, progressChangedCallCount);

            npt.Progress = 1;
            Assert.AreEqual(2, progressChangedCallCount);
        }

        [Test]
        public void IndeterminateTimeEstimateByDefault() {
            var npt = new NotifyingProgressTracker();

            Assert.IsTrue(npt.EstimatedTimeRemaining.IsIndeterminate);
        }

        [Test]
        public void NullTimeEstimateThrows() {
            Assert.Throws<ArgumentNullException>(() => new NotifyingProgressTracker(null));
        }

        [Test]
        public void TimeEstimateFromConstructor() {
            var timeEstimate = TimeEstimates.LongOperation;

            var npt = new NotifyingProgressTracker(timeEstimate);

            Assert.AreSame(timeEstimate, npt.EstimatedTimeRemaining);
        }
    }
}
