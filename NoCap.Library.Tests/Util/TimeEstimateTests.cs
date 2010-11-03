using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Util {
    [TestFixture]
    class TimeEstimateTests {
        [Test]
        public void BuiltInWeightOrder() {
            Assert.Less(TimeEstimates.Instantanious.ProgressWeight, TimeEstimates.UserInteractive.ProgressWeight);
            Assert.Less(TimeEstimates.Instantanious.ProgressWeight, TimeEstimates.ShortOperation.ProgressWeight);
            Assert.Less(TimeEstimates.Instantanious.ProgressWeight, TimeEstimates.LongOperation.ProgressWeight);

            Assert.Less(TimeEstimates.ShortOperation.ProgressWeight, TimeEstimates.LongOperation.ProgressWeight);
        }

        [Test]
        public void BuildInsIndeterminant() {
            Assert.IsFalse(TimeEstimates.Instantanious.IsIndeterminant);
            Assert.IsFalse(TimeEstimates.ShortOperation.IsIndeterminant);
            Assert.IsFalse(TimeEstimates.LongOperation.IsIndeterminant);

            Assert.IsTrue(TimeEstimates.UserInteractive.IsIndeterminant);
            Assert.IsTrue(TimeEstimates.Indeterminant.IsIndeterminant);
        }
    }
}
