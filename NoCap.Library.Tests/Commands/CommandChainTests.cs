using System;
using Moq;
using NoCap.Library.Commands;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Commands {
    [TestFixture]
    class CommandChainTests {
        [Test]
        public void RouteChecksTypes1() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            commandMock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);
            commandMock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var chain = new CommandChain();
            chain.Add(commandMock.Object);

            chain.Process(inputData, inputTracker);

            commandMock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }
        
        [Test]
        public void RouteThrowsOnTypeMismatch1() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });
            commandMock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);
            commandMock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var chain = new CommandChain();
            chain.Add(commandMock.Object);

            Assert.Throws<InvalidOperationException>(() => chain.Process(inputData, inputTracker));
        }
        
        [Test]
        public void RouteReturnsData1() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            commandMock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(expectedOutput);
            commandMock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var chain = new CommandChain();
            chain.Add(commandMock.Object);

            var actualData = chain.Process(inputData, inputTracker);
            Assert.AreSame(expectedOutput, actualData);
        }
        
        [Test]
        public void RouteChecksTypes2() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            command1Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            command2Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            chain.Process(inputData, inputTracker);
            
            command1Mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
            command2Mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void RouteThrowsOnTypeMismatch2A() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });
            command1Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            command2Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            Assert.Throws<InvalidOperationException>(() => chain.Process(inputData, inputTracker));
        }

        [Test]
        public void RouteThrowsOnTypeMismatch2B() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            command1Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });
            command2Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            Assert.Throws<InvalidOperationException>(() => chain.Process(inputData, inputTracker));
        }

        [Test]
        public void RouteReturnsData2() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            command1Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);
            command1Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            command2Mock.Setup((command) => command.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(expectedOutput);
            command2Mock.Setup((command) => command.ProcessTimeEstimate).Returns(TimeEstimates.Instantanious);

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            var actualOutput = chain.Process(inputData, inputTracker);
            Assert.AreSame(expectedOutput, actualOutput);
        }

        [Test]
        public void GetInputDataTypesNone() {
            var chain = new CommandChain();

            CollectionAssert.AreEquivalent(new TypedDataType[] { }, chain.GetInputDataTypes());
        }

        [Test]
        public void GetInputDataTypesGivesFirst() {
            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Image, TypedDataType.Uri });

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            CollectionAssert.AreEquivalent(new[] { TypedDataType.Image, TypedDataType.Uri }, chain.GetInputDataTypes());
        }

        [Test]
        public void GetOutputDataTypes0() {
            var chain = new CommandChain();

            CollectionAssert.AreEquivalent(new[] { TypedDataType.None }, chain.GetOutputDataTypes(TypedDataType.None));
            CollectionAssert.AreEquivalent(new[] { TypedDataType.Text }, chain.GetOutputDataTypes(TypedDataType.Text));
        }

        [Test]
        public void GetOutputDataTypes1() {
            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text, TypedDataType.Uri });
            commandMock.Setup((command) => command.GetOutputDataTypes(TypedDataType.Text)).Returns(new[] { TypedDataType.Image, TypedDataType.Uri });
            commandMock.Setup((command) => command.GetOutputDataTypes(TypedDataType.Uri)).Returns(new[] { TypedDataType.Text, TypedDataType.Uri });

            var chain = new CommandChain();
            chain.Add(commandMock.Object);

            CollectionAssert.AreEquivalent(new[] { TypedDataType.Image, TypedDataType.Uri }, chain.GetOutputDataTypes(TypedDataType.Text));
            CollectionAssert.AreEquivalent(new[] { TypedDataType.Text, TypedDataType.Uri }, chain.GetOutputDataTypes(TypedDataType.Uri));
        }

        [Test]
        public void GetOutputDataTypesBadInputThrows1() {
            var commandMock = GetCommandMock();
            commandMock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            commandMock.Setup((command) => command.GetOutputDataTypes(TypedDataType.Text)).Returns(new[] { TypedDataType.Image, TypedDataType.Uri });

            var chain = new CommandChain();
            chain.Add(commandMock.Object);

            Assert.Throws<InvalidOperationException>(() => chain.GetOutputDataTypes(TypedDataType.Uri));
        }

        [Test]
        public void GetOutputDataTypes2() {
            var command1Mock = GetCommandMock();
            command1Mock.Setup((command) => command.GetInputDataTypes()).Returns(new TypedDataType[] { });
            command1Mock.Setup((command) => command.GetOutputDataTypes(TypedDataType.None)).Returns(new[] { TypedDataType.User + 1, TypedDataType.User + 2, TypedDataType.User + 3 });

            var command2Mock = GetCommandMock();
            command2Mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.User + 1, TypedDataType.User + 3 });
            command2Mock.Setup((command) => command.GetOutputDataTypes(TypedDataType.User + 1)).Returns(new[] { TypedDataType.User + 1, TypedDataType.User + 4 });
            command2Mock.Setup((command) => command.GetOutputDataTypes(TypedDataType.User + 3)).Returns(new[] { TypedDataType.User + 5 });

            var chain = new CommandChain();
            chain.Add(command1Mock.Object);
            chain.Add(command2Mock.Object);

            CollectionAssert.AreEquivalent(new[] { TypedDataType.User + 1, TypedDataType.User + 4, TypedDataType.User + 5 }, chain.GetOutputDataTypes(TypedDataType.None));
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
