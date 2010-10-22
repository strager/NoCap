﻿using System;
using Moq;
using NoCap.Library.Processors;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Processors {
    [TestFixture]
    class ProcessorChainTests {
        [Test]
        public void RouteChecksTypes1() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var processorMock = GetProcessorMock();
            processorMock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processorMock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);

            var chain = new ProcessorChain();
            chain.Add(processorMock.Object);

            chain.Process(inputData, inputTracker);

            processorMock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }
        
        [Test]
        public void RouteThrowsOnTypeMismatch1() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var processorMock = GetProcessorMock();
            processorMock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });
            processorMock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);

            var chain = new ProcessorChain();
            chain.Add(processorMock.Object);

            Assert.Throws<InvalidOperationException>(() => chain.Process(inputData, inputTracker));
        }
        
        [Test]
        public void RouteReturnsData1() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var processorMock = GetProcessorMock();
            processorMock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processorMock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(expectedOutput);

            var chain = new ProcessorChain();
            chain.Add(processorMock.Object);

            var actualData = chain.Process(inputData, inputTracker);
            Assert.AreSame(expectedOutput, actualData);
        }
        
        [Test]
        public void RouteChecksTypes2() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var processor1Mock = GetProcessorMock();
            processor1Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processor1Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);

            var processor2Mock = GetProcessorMock();
            processor2Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processor2Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);

            var chain = new ProcessorChain();
            chain.Add(processor1Mock.Object);
            chain.Add(processor2Mock.Object);

            chain.Process(inputData, inputTracker);
            
            processor1Mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
            processor2Mock.Verify((processor) => processor.GetInputDataTypes(), Times.AtLeastOnce());
        }

        [Test]
        public void RouteThrowsOnTypeMismatch2A() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var processor1Mock = GetProcessorMock();
            processor1Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });
            processor1Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);

            var processor2Mock = GetProcessorMock();
            processor2Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processor2Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);

            var chain = new ProcessorChain();
            chain.Add(processor1Mock.Object);
            chain.Add(processor2Mock.Object);

            Assert.Throws<InvalidOperationException>(() => chain.Process(inputData, inputTracker));
        }

        [Test]
        public void RouteThrowsOnTypeMismatch2B() {
            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var processor1Mock = GetProcessorMock();
            processor1Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processor1Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);

            var processor2Mock = GetProcessorMock();
            processor2Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Uri });
            processor2Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns((TypedData) null);

            var chain = new ProcessorChain();
            chain.Add(processor1Mock.Object);
            chain.Add(processor2Mock.Object);

            Assert.Throws<InvalidOperationException>(() => chain.Process(inputData, inputTracker));
        }

        [Test]
        public void RouteReturnsData2() {
            var expectedOutput = GetTextData();

            var inputTracker = GetMutableProgressTracker();
            var inputData = GetTextData();

            var processor1Mock = GetProcessorMock();
            processor1Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processor1Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(inputData);

            var processor2Mock = GetProcessorMock();
            processor2Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });
            processor2Mock.Setup((processor) => processor.Process(inputData, It.IsAny<IMutableProgressTracker>())).Returns(expectedOutput);

            var chain = new ProcessorChain();
            chain.Add(processor1Mock.Object);
            chain.Add(processor2Mock.Object);

            var actualOutput = chain.Process(inputData, inputTracker);
            Assert.AreSame(expectedOutput, actualOutput);
        }

        [Test]
        public void GetInputDataTypesNone() {
            var chain = new ProcessorChain();

            CollectionAssert.AreEquivalent(new TypedDataType[] { }, chain.GetInputDataTypes());
        }

        [Test]
        public void GetInputDataTypesGivesFirst() {
            var processor1Mock = GetProcessorMock();
            processor1Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Image, TypedDataType.Uri });

            var processor2Mock = GetProcessorMock();
            processor2Mock.Setup((processor) => processor.GetInputDataTypes()).Returns(new[] { TypedDataType.Text });

            var chain = new ProcessorChain();
            chain.Add(processor1Mock.Object);
            chain.Add(processor2Mock.Object);

            CollectionAssert.AreEquivalent(new[] { TypedDataType.Image, TypedDataType.Uri }, chain.GetInputDataTypes());
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
