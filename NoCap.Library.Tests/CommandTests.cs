using System;
using Moq;
using NUnit.Framework;

namespace NoCap.Library.Tests {
    [TestFixture]
    class CommandTests {
        [Test]
        public void IsValidInputTypeTrue() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            bool isValid = mock.Object.IsValidInputType(GetTextData());

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidInputTypeFalse() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });

            bool isValid = mock.Object.IsValidInputType(GetTextData());

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsFalse(isValid);
        }

        [Test]
        public void IsValidInputTypeTrueEmpty() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new TypedDataType[] { });

            bool isValid = mock.Object.IsValidInputType(GetNullData());

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidInputTypeTrueNone() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.None });

            bool isValid = mock.Object.IsValidInputType(GetNullData());

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidInputTypeFalseEmpty() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new TypedDataType[] { });

            bool isValid = mock.Object.IsValidInputType(GetTextData());

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsFalse(isValid);
        }

        [Test]
        public void CheckValidInputTypeDoesNotThrow() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            Assert.DoesNotThrow(() => mock.Object.CheckValidInputType(GetTextData()));

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeThrows() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });

            Assert.Throws<InvalidOperationException>(() => mock.Object.CheckValidInputType(GetTextData()));

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeDoesNotThrowEmpty() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new TypedDataType[] { });

            Assert.DoesNotThrow(() => mock.Object.CheckValidInputType(GetNullData()));

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeDoesNotThrowNone() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new[] { TypedDataType.None });

            Assert.DoesNotThrow(() => mock.Object.CheckValidInputType(GetNullData()));

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeThrowsEmpty() {
            var mock = GetCommandMock();
            mock.Setup((command) => command.GetInputDataTypes()).Returns(new TypedDataType[] { });

            Assert.Throws<InvalidOperationException>(() => mock.Object.CheckValidInputType(GetTextData()));

            mock.Verify((command) => command.GetInputDataTypes(), Times.AtLeastOnce());
        }

        private static Mock<ICommand> GetCommandMock() {
            return new Mock<ICommand>(MockBehavior.Strict);
        }

        private static TypedData GetNullData() {
            return new TypedData(TypedDataType.None, null, "null data");
        }

        private static TypedData GetTextData() {
            return TypedData.FromText("foobar", "text data");
        }
    }
}
