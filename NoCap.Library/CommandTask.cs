using System;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library {
    public class CommandTask {
        private readonly object syncRoot = new object();

        private readonly ICommand command;
        private readonly CommandRunner commandRunner;
        private IMutableProgressTracker progressTracker;
        private Thread thread;

        private bool isCompleted;
        private CommandCancelledException cancellation;

        internal CommandTask(ICommand command, CommandRunner commandRunner) {
            this.command = command;
            this.commandRunner = commandRunner;
        }

        internal void Run() {
            lock (this.syncRoot) {
                if (IsRunning) {
                    throw new InvalidOperationException("Task already running");
                }

                this.isCompleted = false;

                this.thread = new Thread(RunThread);
                this.thread.Start();
            }

            this.commandRunner.OnTaskStarted(new CommandTaskEventArgs(this));
        }

        private void RunThread() {
            this.progressTracker = new NotifyingProgressTracker();
            this.progressTracker.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    this.commandRunner.OnProgressUpdated(new CommandTaskProgressEventArgs(this, this.progressTracker.Progress));
                }
            };

            try {
                using (command.Process(null, this.progressTracker)) {
                    // Auto-dispose
                }
            } catch (CommandCancelledException e) {
                Cancelled(e);
            }

            Completed();
        }

        private void Cancelled(CommandCancelledException cancelReason) {
            lock (this.syncRoot) {
                this.cancellation = cancelReason;
                this.isCompleted = true;
            }

            this.commandRunner.OnTaskCancelled(new CommandTaskCancellationEventArgs(this, cancelReason));
        }

        private void Completed() {
            lock (this.syncRoot) {
                this.isCompleted = true;
            }

            this.commandRunner.OnTaskCompleted(new CommandTaskEventArgs(this));
        }

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public bool IsRunning {
            get {
                lock (this.syncRoot) {
                    return this.thread != null && this.thread.IsAlive;
                }
            }
        }

        public bool IsCompleted {
            get {
                lock (this.syncRoot) {
                    return this.isCompleted;
                }
            }
        }

        public bool IsCancelled {
            get {
                lock (this.syncRoot) {
                    return this.cancellation != null;
                }
            }
        }

        public CommandCancelledException CancelReason {
            get {
                lock (this.syncRoot) {
                    if (this.cancellation == null) {
                        throw new InvalidOperationException("Task was not cancelled");
                    }

                    return this.cancellation;
                }
            }
        }

        public void WaitForCompletion() {
            if (IsCompleted) {
                return;
            }

            if (!IsRunning) {
                throw new InvalidOperationException("Task not running");
            }

            this.thread.Join();
        }
    }
}