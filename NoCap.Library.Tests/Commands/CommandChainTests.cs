using System;
using System.Threading;
using Moq;
using NoCap.Library.Commands;
using NoCap.Library.Tests.TestHelpers;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Commands {
    [TestFixture]
    class CommandChainTests {
        [Test]
        public void RouteReturnsData1() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>(), CancellationToken.None)).Returns(expectedOutput);
            commandMock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantaneous);

            var chain = new CommandChain();
            chain.Add(commandMock.Object);

            var actualData = chain.Process(inputData, inputTracker, CancellationToken.None);
            Assert.AreSame(expectedOutput, actualData);
        }

        [Test]
        public void RouteReturnsData2() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>(), CancellationToken.None)).Returns(inputData);
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantaneous);

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>(), CancellationToken.None)).Returns(expectedOutput);
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantaneous);

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            var actualOutput = chain.Process(inputData, inputTracker, CancellationToken.None);
            Assert.AreSame(expectedOutput, actualOutput);
        }

        [Test]
        public void TimeEstimateWeightIsSumOfChildWeights() {
            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(9));
            
            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(20));

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            Assert.AreEqual(29, chain.ProcessTimeEstimate.ProgressWeight);
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
