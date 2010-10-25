using System;
using Moq;
using NoCap.Library.Processors;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Processors {
    [TestFixture]
    class DataRouterTests {
        [Test]
        public void ConnectChecksTypes() {
            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, destination.Object);

            destination.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void ConnectThrowsOnTypeMismatch() {
            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Image });

            var dataRouter = new DataRouter();

            Assert.Throws<ArgumentException>(() => dataRouter.Connect(TypedDataType.Text, destination.Object));

            destination.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }
        
        [Test]
        public void ProcessChecksTypes() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.Process(inputData, inputTracker)).Returns((TypedData) null);
            
            // .Connect checks the type too, so we make sure it passes .Connect's check first
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, destination.Object);

            // No need to change the type here

            dataRouter.Process(inputData, inputTracker);

            // Two calls: one from .Connect, one from .Process
            destination.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeast(2));
        }
        
        [Test]
        public void ProcessThrowsOnTypeMismatch() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var destination = GetProcessorMock();
            
            // .Connect checks the type too, so we make sure it passes .Connect's check first
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, destination.Object);

            // Change the types the processor accepts so the type check fails
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });

            Assert.Throws<InvalidOperationException>(() => dataRouter.Process(inputData, inputTracker));

            // Two calls: one from .Connect, one from .Process
            destination.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeast(2));
        }

        [Test]
        public void ProcessCallsRoutedProcess() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            destination.Setup((processor) => processor.Process(inputData, inputTracker)).Returns((TypedData) null);

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, destination.Object);

            dataRouter.Process(inputData, inputTracker);

            destination.Verify((processor) => processor.Process(inputData, inputTracker), Times.Once());
        }

        [Test]
        public void ProcessReturnsData() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            destination.Setup((processor) => processor.Process(inputData, inputTracker)).Returns(expectedOutput);

            var dataRouter = new DataRouter();
            dataRouter.Connect(TypedDataType.Text, destination.Object);

            var actualOutput = dataRouter.Process(inputData, inputTracker);
            Assert.AreSame(expectedOutput, actualOutput);
        }

        private static Mock<IProcessor> GetProcessorMock() {
            return new Mock<IProcessor>(MockBehavior.Strict);
        }

        private static NotifyingProgressTracker GetMutableProgressTracker() {
            return new NotifyingProgressTracker();
        }

        private static TypedData GetTextData() {
            return TypedData.FromText("foobar", "test text");
        }
    }
}
