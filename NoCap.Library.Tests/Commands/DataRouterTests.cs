using System;
using System.Threading;
using Moq;
using NoCap.Library.Commands;
using NoCap.Library.Tests.TestHelpers;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Commands {
    [TestFixture]
    class DataRouterTests {
        [Test]
        public void ConnectChecksTypes() {
            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            commandMock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void ConnectThrowsOnTypeMismatch() {
            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Image });

            var dataRouter = new DataRouter();

            Assert.Throws<ArgumentException>(() => dataRouter.Connect(TypedDataType.Text, commandMock.Object));

            commandMock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }
        
        [Test]
        public void ProcessChecksTypes() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.Process(inputData, inputTracker, CancellationToken.None)).Returns((TypedData) null);
            
            // .Connect checks the type too, so we make sure it passes .Connect's check first
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            // No need to change the type here

            dataRouter.Process(inputData, inputTracker, CancellationToken.None);

            // Two calls: one from .Connect, one from .Process
            commandMock.Verify((command) => command.GetInputDataTypes(), Times.AtLeast(2));
        }
        
        [Test]
        public void ProcessThrowsOnTypeMismatch() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            
            // .Connect checks the type too, so we make sure it passes .Connect's check first
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            // Change the types the command accepts so the type check fails
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });

            Assert.Throws<InvalidOperationException>(() => dataRouter.Process(inputData, inputTracker, CancellationToken.None));

            // Two calls: one from .Connect, one from .Process
            commandMock.Verify((command) => command.GetInputDataTypes(), Times.AtLeast(2));
        }

        [Test]
        public void ProcessCallsRoutedProcess() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
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
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
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
            command1Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            
            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(20));
            command2Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });

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
