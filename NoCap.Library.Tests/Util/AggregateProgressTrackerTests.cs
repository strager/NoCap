using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Util {
    [TestFixture]
    public class AggregateProgressTrackerTests {
        [Test]
        public void ProgressIsAverage() {
            var npts = new[] {
                new NotifyingProgressTracker(),
                new NotifyingProgressTracker(),
                new NotifyingProgressTracker(),
                new NotifyingProgressTracker()
            };

            var apt = new AggregateProgressTracker(npts);

            Assert.AreEqual(0, apt.Progress);
            
            npts = new[] {
                new NotifyingProgressTracker { Progress = 1 },
                new NotifyingProgressTracker { Progress = 0.5 },
                new NotifyingProgressTracker { Progress = 0.5 },
                new NotifyingProgressTracker { Progress = 0 }
            };

            apt = new AggregateProgressTracker(npts);

            Assert.AreEqual((1 + 0.5 + 0.5 + 0) / 4, apt.Progress);

            npts = new[] {
                new NotifyingProgressTracker { Progress = 1 },
                new NotifyingProgressTracker { Progress = 1 },
                new NotifyingProgressTracker { Progress = 1 },
                new NotifyingProgressTracker { Progress = 1 }
            };

            apt = new AggregateProgressTracker(npts);

            Assert.AreEqual(1, apt.Progress);
        }

        [Test]
        public void ProgressChangesWhenChildChanges() {
            var npts = new[] {
                new NotifyingProgressTracker(),
                new NotifyingProgressTracker(),
                new NotifyingProgressTracker(),
                new NotifyingProgressTracker()
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
    }
}
