using System;
using Moq;
using NUnit.Framework;

namespace NoCap.Library.Tests {
    [TestFixture]
    class ProcessorTests {
        [Test]
        public void GetEffectiveDataTypeOnNull() {
            var type = Processor.GetEffectiveDataType(null);

            Assert.AreEqual(TypedDataType.None, type);
        }

        [Test]
        public void GetEffectiveDataTypeOnNone() {
            var type = Processor.GetEffectiveDataType(GetNullData());

            Assert.AreEqual(TypedDataType.None, type);
        }

        [Test]
        public void GetEffectiveDataTypeOnText() {
            var type = Processor.GetEffectiveDataType(GetTextData());

            Assert.AreEqual(TypedDataType.Text, type);
        }

        [Test]
        public void IsValidInputTypeTrue() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            bool isValid = mock.Object.IsValidInputType(GetTextData());

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidInputTypeFalse() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });

            bool isValid = mock.Object.IsValidInputType(GetTextData());

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsFalse(isValid);
        }

        [Test]
        public void IsValidInputTypeTrueEmpty() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new TypedDataType[] { });

            bool isValid = mock.Object.IsValidInputType(GetNullData());

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidInputTypeTrueNone() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.None });

            bool isValid = mock.Object.IsValidInputType(GetNullData());

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsTrue(isValid);
        }

        [Test]
        public void IsValidInputTypeFalseEmpty() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new TypedDataType[] { });

            bool isValid = mock.Object.IsValidInputType(GetTextData());

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());

            Assert.IsFalse(isValid);
        }

        [Test]
        public void CheckValidInputTypeDoesNotThrow() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            Assert.DoesNotThrow(() => mock.Object.CheckValidInputType(GetTextData()));

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeThrows() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });

            Assert.Throws<InvalidOperationException>(() => mock.Object.CheckValidInputType(GetTextData()));

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeDoesNotThrowEmpty() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new TypedDataType[] { });

            Assert.DoesNotThrow(() => mock.Object.CheckValidInputType(GetNullData()));

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeDoesNotThrowNone() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.None });

            Assert.DoesNotThrow(() => mock.Object.CheckValidInputType(GetNullData()));

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void CheckValidInputTypeThrowsEmpty() {
            var mock = GetProcessorMock();
            mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new TypedDataType[] { });

            Assert.Throws<InvalidOperationException>(() => mock.Object.CheckValidInputType(GetTextData()));

            mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        // TODO Test IsValidInputOutputType

        private static Mock<IProcessor> GetProcessorMock() {
            return new Mock<IProcessor>(MockBehavior.Strict);
        }

        private static TypedData GetNullData() {
            return new TypedData(TypedDataType.None, null, "null data");
        }

        private static TypedData GetTextData() {
            return TypedData.FromText("foobar", "text data");
        }
    }
}
