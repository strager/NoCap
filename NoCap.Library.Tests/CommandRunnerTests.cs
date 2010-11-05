using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Moq;
using NoCap.Library.Commands;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Library.Tests {
    [TestFixture]
    class CommandRunnerTests {
        [Test]
        public void RunThrowsOnNull() {
            var runner = new CommandRunner();

            Assert.Throws<ArgumentNullException>(() => runner.Run(null));
        }

        [Test, MaxTime(1000)]
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

        [Test, MaxTime(1000)]
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

        [Test, MaxTime(1000)]
        public void RunExecutesCommand() {
            var runner = new CommandRunner();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null))).Returns((TypedData) null);

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            mockCommand.Verify((command) => command.Process(null, It.IsAny<IMutableProgressTracker>()), Times.Once());
        }

        [Test, MaxTime(1000)]
        public void RunExecutesCommandInSeparateThread() {
            var runner = new CommandRunner();
            Thread commandThread = null;

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null))).Returns((TypedData) null)
                .Callback(() => {
                    commandThread = Thread.CurrentThread;
                });

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.AreNotEqual(Thread.CurrentThread, commandThread);
        }

        [Test, MaxTime(1000)]
        public void ProgressUpdatesFire() {
            var runner = new CommandRunner();

            var progressUpdates = new List<double>();

            runner.ProgressUpdated += (sender, e) => progressUpdates.Add(e.Progress);

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null))).Returns((TypedData) null)
                .Callback((TypedData data, IMutableProgressTracker progress) => {
                    progress.Progress = 0.5;
                    progress.Progress = 0.6;
                    progress.Progress = 1;
                });

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            CollectionAssert.AreEqual(new[] { 0.5, 0.6, 1 }, progressUpdates);
        }

        [Test, MaxTime(1000)]
        public void RunSetsIsRunning() {
            var runner = new CommandRunner();

            CommandTask task = null;
            bool? isRunningInCommand = null;

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null))).Returns((TypedData) null)
                .Callback(() => {
                    isRunningInCommand = task.IsRunning;
                });

            task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.IsNotNull(isRunningInCommand);
            Assert.IsTrue((bool) isRunningInCommand);
        }

        [Test, MaxTime(1000)]
        public void NotCompletedDuringRun() {
            var runner = new CommandRunner();

            CommandTask task = null;
            bool? isCompletedInCommand = null;

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null))).Returns((TypedData) null)
                .Callback(() => {
                    isCompletedInCommand = task.IsCompleted;
                });

            task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.IsNotNull(isCompletedInCommand);
            Assert.IsFalse((bool) isCompletedInCommand);
        }

        [Test, MaxTime(1000)]
        public void RunEndResetsIsRunning() {
            var runner = new CommandRunner();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null))).Returns((TypedData) null);

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.IsFalse(task.IsRunning);
        }

        [Test, MaxTime(1000)]
        public void CompletedAfterRun() {
            var runner = new CommandRunner();

            var mockCommand = GetCommandMock();
            mockCommand.Setup((command) => command.Process(null, It.Is<IMutableProgressTracker>((mpt) => mpt != null))).Returns((TypedData) null);

            var task = runner.Run(mockCommand.Object);
            task.WaitForCompletion();

            Assert.IsTrue(task.IsCompleted);
        }

        private static Mock<ICommand> GetCommandMock() {
            return new Mock<ICommand>(MockBehavior.Strict);
        }
    }
}
