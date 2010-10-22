using System;
using Moq;
using NoCap.Library.Processors;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Processors {
    [TestFixture]
    class DataRouterTests {
        [Test]
        public void AddChecksTypes() {
            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var dataRouter = new DataRouter();
            dataRouter.Add(TypedDataType.Text, destination.Object);

            destination.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void AddThrowsOnTypeMismatch() {
            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Image });

            var dataRouter = new DataRouter();

            Assert.Throws<ArgumentException>(() => dataRouter.Add(TypedDataType.Text, destination.Object));

            destination.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void RouteCallsProcess() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            destination.Setup((processor) => processor.Process(inputData, inputTracker)).Returns((TypedData) null);

            var dataRouter = new DataRouter();
            dataRouter.Add(TypedDataType.Text, destination.Object);

            dataRouter.Process(inputData, inputTracker);

            destination.Verify((processor) => processor.Process(inputData, inputTracker), Times.Once());
        }

        [Test]
        public void RouteReturnsData() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var destination = GetProcessorMock();
            destination.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            destination.Setup((processor) => processor.Process(inputData, inputTracker)).Returns(expectedOutput);

            var dataRouter = new DataRouter();
            dataRouter.Add(TypedDataType.Text, destination.Object);

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
