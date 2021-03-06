﻿using System;
using System.Threading;
using Moq;
using NoCap.Library.Commands;
using NoCap.Library.Progress;
using NoCap.Library.Tests.TestHelpers;
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

            var chain = new CommandChain(commandMock.Object);

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

            var chain = new CommandChain(command1Mock.Object, command2Mock.Object);

            var actualOutput = chain.Process(inputData, inputTracker, CancellationToken.None);
            Assert.AreSame(expectedOutput, actualOutput);
        }

        [Test]
        public void TimeEstimateWeightIsSumOfChildWeights() {
            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(9));
            
            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(20));

            var chain = new CommandChain(command1Mock.Object, command2Mock.Object);

            Assert.AreEqual(29, chain.ProcessTimeEstimate.ProgressWeight);
        }

        [Test]
        public void ProgressUpdatePropogates() {
            var inputTracker = GetMutableProgressTracker();

            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.Process(null, It.IsAny<IMutableProgressTracker>(), CancellationToken.None))
                .Returns((TypedData) null)
                .Callback((TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) => {
                    bool updateCalled = false;

                    EventHandler<ProgressUpdatedEventArgs> handler = (sender, e) => {
                        Assert.AreEqual(0.25, e.Progress, 0.001);

                        updateCalled = true;
                    };

                    inputTracker.ProgressUpdated += handler;

                    progress.Progress = 0.5;

                    inputTracker.ProgressUpdated -= handler;

                    Assert.IsTrue(updateCalled);
                });
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(10));

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.Process(null, It.IsAny<IMutableProgressTracker>(), CancellationToken.None)).Returns((TypedData) null);
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(new TestTimeEstimate(10));

            var chain = new CommandChain(command1Mock.Object, command2Mock.Object);

            chain.Process(null, inputTracker, CancellationToken.None);
        }

        private static Mock<ICommand> GetCommandMock() {
            return new Mock<ICommand>(MockBehavior.Strict);
        }

        private static MutableProgressTracker GetMutableProgressTracker() {
            return new MutableProgressTracker();
        }

        private static TypedData GetTextData() {
            return TypedData.FromText("foobar", "test text");
        }
    }
}
