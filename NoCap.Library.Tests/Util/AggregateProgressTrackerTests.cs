using NoCap.Library.Progress;
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
            
            ((IMutableProgressTracker) npts[0].ProgressTracker).Progress = 1;
            Assert.AreEqual(1 / 4.0, apt.Progress);

            ((IMutableProgressTracker) npts[1].ProgressTracker).Progress = 1;
            Assert.AreEqual(2 / 4.0, apt.Progress);

            ((IMutableProgressTracker) npts[2].ProgressTracker).Progress = 1;
            Assert.AreEqual(3 / 4.0, apt.Progress);

            ((IMutableProgressTracker) npts[3].ProgressTracker).Progress = 1;
            Assert.AreEqual(4 / 4.0, apt.Progress);

            ((IMutableProgressTracker) npts[2].ProgressTracker).Progress = 0.5;
            Assert.AreEqual(7 / 8.0, apt.Progress);
        }

        private static AggregateProgressTrackerInformation GetTracker(double weight, double progress) {
            return new AggregateProgressTrackerInformation(
                new MutableProgressTracker { Progress = progress },
                weight
            );
        }
    }
}
