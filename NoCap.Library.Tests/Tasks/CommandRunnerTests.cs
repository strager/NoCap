using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NoCap.Library.Commands;
using NoCap.Library.Progress;
using NoCap.Library.Tasks;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests.Tasks {
    [TestFixture]
    class CommandRunnerTests {
        [Test]
        public void RunThrowsOnNull() {
            var runner = new CommandRunner();

            Assert.Throws<ArgumentNullException>(() => runner.Run(null));
        }

        [Test]
        public void RunFiresStartOnce() {
            var runner = new CommandRunner();

            int fireCount = 0;

            runner.TaskStarted += (sender, e) => {
                ++fireCount;
            };

            var task = runner.Run(new CommandChain());
            task.WaitForCompletion();

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void RunFiresCompleteOnce() {
            var runner = new CommandRunner();

            int fireCount = 0;

            runner.TaskCompleted += (sender, e) => {
                ++fireCount;
            };

            var task = runner.Run(new CommandChain());
            task.WaitForCompletion();

            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void RunExecutesCommand() {
            var runner = new CommandRunner();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null);

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            mockCommand.Verify((command) => command.Process(null, It.IsAny<IMutableProgressTracker>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public void RunExecutesCommandInSeparateThread() {
            var runner = new CommandRunner();
            Thread commandThread = null;

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null)
                .Callback(() => {
                    commandThread = Thread.CurrentThread;
                });

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.AreNotEqual(Thread.CurrentThread, commandThread);
        }

        [Test]
        public void ProgressUpdatesFire() {
            var runner = new CommandRunner();

            var progressUpdates = new List<double>();

            runner.ProgressUpdated += (sender, e) => progressUpdates.Add(e.Progress);

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null)
                .Callback((TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) => {
                    progress.Progress = 0.5;
                    progress.Progress = 0.6;
                    progress.Progress = 1;
                });

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            CollectionAssert.AreEqual(new[] { 0.5, 0.6, 1 }, progressUpdates);
        }

        [Test]
        public void RunSetsIsRunning() {
            var runner = new CommandRunner();

            ICommandTask task = null;
            TaskState? taskStateInCommand = null;

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null)
                .Callback(() => {
                    taskStateInCommand = task.State;
                });

            task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.IsNotNull(taskStateInCommand);
            Assert.AreEqual(TaskState.Running, (TaskState) taskStateInCommand);
        }

        [Test]
        public void RunEndResetsIsRunning() {
            var runner = new CommandRunner();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null);

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.AreNotEqual(TaskState.Running, task.State);
        }

        [Test]
        public void CompletedAfterRun() {
            var runner = new CommandRunner();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null);

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.AreEqual(TaskState.Completed, task.State);
        }

        [Test]
        public void RunDisposesReturnedData() {
            var mockDisposable = new Mock<IDisposable>(MockBehavior.Strict);
            mockDisposable.Setup((typedData) => typedData.Dispose());

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns(new TypedData(TypedDataType.User, mockDisposable.Object, "my data"));

            var runner = new CommandRunner();
            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            mockDisposable.Verify((typedData) => typedData.Dispose(), Times.Once());
        }

        [Test]
        public void CancellationSetsCancelled() {
            var runner = new CommandRunner();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null)
                .Callback(() => {
                    throw new CommandCanceledException();
                });

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.AreEqual(TaskState.Canceled, task.State);
        }

        [Test]
        public void CancellationSetsCancelReason() {
            var runner = new CommandRunner();

            var cancelException = new CommandCanceledException();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null)
                .Callback(() => {
                    throw cancelException;
                });

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            AssertCanceledExceptionsSame(cancelException, task.CancelReason);
        }

        [Test]
        public void CancellationFiresComplete() {
            var runner = new CommandRunner();

            var cancelException = new CommandCanceledException();
            CommandCanceledException firedReason = null;

            int fireCount = 0;

            runner.TaskCanceled += (sender, e) => {
                ++fireCount;

                firedReason = e.CancelReason;
            };

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null)
                .Callback(() => {
                    throw cancelException;
                });

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.AreEqual(1, fireCount);
            AssertCanceledExceptionsSame(cancelException, firedReason);
        }

        [Test]
        public void TaskNameIsCommandName() {
            var runner = new CommandRunner();

            const string commandName = "Command name";

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null), It.IsAny<CancellationToken>()))
                .Returns((TypedData) null);
            mockCommand.Setup((command) => command.Name)
                .Returns(commandName);

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.AreEqual(commandName, task.Name);
        }

        private static Mock<ICommand> GetCommandMock() {
            return new Mock<ICommand>(MockBehavior.Strict);
        }

        private void AssertCanceledExceptionsSame(CommandCanceledException expected, Exception actual) {
            if (expected == null && actual == null) {
                return;
            }

            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            while (actual.InnerException != null) {
                actual = actual.InnerException;
            }

            Assert.AreSame(expected, actual);
        }
    }
}
