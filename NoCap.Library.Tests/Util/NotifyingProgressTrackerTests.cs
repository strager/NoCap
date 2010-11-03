using System;
using NoCap.Library.Util;
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

            npt.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    progressChangedCallCount += 1;
                }
            };

            npt.Progress = 1;
            Assert.AreEqual(1, progressChangedCallCount);

            npt.Progress = 1;
            Assert.AreEqual(2, progressChangedCallCount);
        }

        [Test]
        public void IndeterminantTimeEstimateByDefault() {
            var npt = new NotifyingProgressTracker();

            Assert.IsTrue(npt.EstimatedTimeRemaining.IsIndeterminant);
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
