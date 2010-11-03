using Moq;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Util {
    [TestFixture]
    public class AggregateProgressTrackerTests {
        [Test]
        public void ProgressAverage() {
            var npts = new[] {
                GetTracker(1, 0),
                GetTracker(1, 0),
                GetTracker(1, 0),
                GetTracker(1, 0),
            };

            var apt = new AggregateProgressTracker(npts);

            Assert.AreEqual(0, apt.Progress);
            
            npts = new[] {
                GetTracker(1, 1),
                GetTracker(1, 0.5),
                GetTracker(1, 0.5),
                GetTracker(1, 0),
            };

            apt = new AggregateProgressTracker(npts);

            Assert.AreEqual((1 + 0.5 + 0.5 + 0) / 4, apt.Progress);

            npts = new[] {
                GetTracker(1, 1),
                GetTracker(1, 1),
                GetTracker(1, 1),
                GetTracker(1, 1),
            };

            apt = new AggregateProgressTracker(npts);

            Assert.AreEqual(1, apt.Progress);
        }

        [Test]
        public void ProgressUsesWeights() {
            var npts = new[] {
                GetTracker(1, 0.5),
                GetTracker(2, 0.25),
                GetTracker(4, 1),
            };

            var apt = new AggregateProgressTracker(npts);

            Assert.AreEqual((1 * 0.5 + 2 * 0.25 + 4 * 1) / (1 + 2 + 4), apt.Progress);
        }

        [Test]
        public void ProgressChangesWhenChildChanges() {
            var npts = new[] {
                GetTracker(1, 0),
                GetTracker(1, 0),
                GetTracker(1, 0),
                GetTracker(1, 0),
            };

            var apt = new AggregateProgressTracker(npts);

            Assert.AreEqual(0 / 4.0, apt.Progress);
            
            npts[0].Progress = 1;
            Assert.AreEqual(1 / 4.0, apt.Progress);

            npts[1].Progress = 1;
            Assert.AreEqual(2 / 4.0, apt.Progress);

            npts[2].Progress = 1;
            Assert.AreEqual(3 / 4.0, apt.Progress);

            npts[3].Progress = 1;
            Assert.AreEqual(4 / 4.0, apt.Progress);

            npts[2].Progress = 0.5;
            Assert.AreEqual(7 / 8.0, apt.Progress);
        }

        [Test]
        public void TimeEstimateWeightIsSumOfChildWeights() {
            var npts = new[] {
                GetTracker(1, 0.5),
                GetTracker(2, 0.25),
                GetTracker(4, 1),
            };

            var apt = new AggregateProgressTracker(npts);

            Assert.AreEqual(7, apt.EstimatedTimeRemaining.ProgressWeight);
        }

        private static NotifyingProgressTracker GetTracker(double weight, double progress) {
            var timeEstimateMock = new Mock<ITimeEstimate>(MockBehavior.Strict);
            timeEstimateMock.Setup((timeEstimate) => timeEstimate.ProgressWeight).Returns(weight);

            return new NotifyingProgressTracker(timeEstimateMock.Object) {
                Progress = progress
            };
        }
    }
}
