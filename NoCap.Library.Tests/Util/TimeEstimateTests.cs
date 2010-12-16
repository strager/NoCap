using NoCap.Library.Progress;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Util {
    [TestFixture]
    class TimeEstimateTests {
        [Test]
        public void BuiltInWeightOrder() {
            Assert.Less(TimeEstimates.Instantaneous.ProgressWeight, TimeEstimates.UserInteractive.ProgressWeight);
            Assert.Less(TimeEstimates.Instantaneous.ProgressWeight, TimeEstimates.ShortOperation.ProgressWeight);
            Assert.Less(TimeEstimates.Instantaneous.ProgressWeight, TimeEstimates.LongOperation.ProgressWeight);

            Assert.Less(TimeEstimates.ShortOperation.ProgressWeight, TimeEstimates.LongOperation.ProgressWeight);
        }

        [Test]
        public void BuildInsIndeterminate() {
            Assert.IsFalse(TimeEstimates.Instantaneous.IsIndeterminate);
            Assert.IsFalse(TimeEstimates.ShortOperation.IsIndeterminate);
            Assert.IsFalse(TimeEstimates.LongOperation.IsIndeterminate);

            Assert.IsTrue(TimeEstimates.UserInteractive.IsIndeterminate);
            Assert.IsTrue(TimeEstimates.Indeterminate.IsIndeterminate);
        }
    }
}
