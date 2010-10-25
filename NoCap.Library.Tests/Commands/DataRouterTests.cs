using System;
using Moq;
using NoCap.Library.Commands;
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
            commandMock.Setup((command) => command.Process(inputData, inputTracker)).Returns((TypedData) null);
            
            // .Connect checks the type too, so we make sure it passes .Connect's check first
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            // No need to change the type here

            dataRouter.Process(inputData, inputTracker);

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

            Assert.Throws<InvalidOperationException>(() => dataRouter.Process(inputData, inputTracker));

            // Two calls: one from .Connect, one from .Process
            commandMock.Verify((command) => command.GetInputDataTypes(), Times.AtLeast(2));
        }

        [Test]
        public void ProcessCallsRoutedProcess() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            commandMock.Setup((command) => command.Process(inputData, inputTracker)).Returns((TypedData) null);

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            dataRouter.Process(inputData, inputTracker);

            commandMock.Verify((command) => command.Process(inputData, inputTracker), Times.Once());
        }

        [Test]
        public void ProcessReturnsData() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            commandMock.Setup((command) => command.Process(inputData, inputTracker)).Returns(expectedOutput);

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, commandMock.Object);

            var actualOutput = dataRouter.Process(inputData, inputTracker);
            Assert.AreSame(expectedOutput, actualOutput);
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
