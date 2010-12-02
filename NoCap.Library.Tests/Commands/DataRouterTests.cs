using System;
using System.Threading;
using Moq;
using NoCap.Library.Commands;
using NoCap.Library.Progress;
using NoCap.Library.Tests.TestHelpers;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Commands {
    [TestFixture]
    class DataRouterTests {
        [Test]
        public void ProcessCallsRoutedProcess() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.Process(inputData, inputTracker, CancellationToken.None)).Returns((TypedData) null);

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            dataRouter.Process(inputData, inputTracker, CancellationToken.None);

            commandMock.Verify((command) => command.Process(inputData, inputTracker, CancellationToken.None), Times.Once());
        }

        [Test]
        public void ProcessReturnsData() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.Process(inputData, inputTracker, CancellationToken.None)).Returns(expectedOutput);

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            var actualOutput = dataRouter.Process(inputData, inputTracker, CancellationToken.None);
            Assert.AreSame(expectedOutput, actualOutput);
        }

        [Test]
        public void TimeEstimateWeightIsMaximumOfChildWeights() {
            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(9));
            
            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(20));

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, command1Mock.Object);
            dataRouter.Connect(TypedDataType.Uri, command2Mock.Object);

            Assert.AreEqual(20, dataRouter.ProcessTimeEstimate.ProgressWeight);
        }

        [Test]
        public void NoTimeEstimateWeightIfNoRoutes() {
            var dataRouter = new DataRouter();

            Assert.AreEqual(0, dataRouter.ProcessTimeEstimate.ProgressWeight);
        }

        private static Mock<ICommand> GetCommandMock() {
            return new Mock<ICommand>(MockBehavior.Strict);
        }

        private static NotifyingProgressTracker GetMutableProgressTracker() {
            return new NotifyingProgressTracker();
        }

        private static TypedData GetTextData() {
            return TypedData.FromText("foobar", "test text");
        }
    }
}
