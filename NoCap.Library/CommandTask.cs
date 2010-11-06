using System;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library {
    public sealed class CommandTask {
        private readonly object syncRoot = new object();

        private readonly ICommand command;
        private readonly CommandRunner commandRunner;
        private IMutableProgressTracker progressTracker;
        private Thread thread;

        private bool isCompleted;
        private CommandCancelledException cancellation;

        public event EventHandler<CommandTaskEventArgs> Started;
        public event EventHandler<CommandTaskEventArgs> Completed;
        public event EventHandler<CommandTaskCancellationEventArgs> Cancelled;
        public event EventHandler<CommandTaskProgressEventArgs> ProgressUpdated;

        internal CommandTask(ICommand command, CommandRunner commandRunner) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            if (commandRunner == null) {
                throw new ArgumentNullException("commandRunner");
            }

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

            OnStarted();
        }

        private void RunThread() {
            this.progressTracker = new NotifyingProgressTracker();
            this.progressTracker.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    OnProgressUpdated();
                }
            };

            try {
                using (command.Process(null, this.progressTracker)) {
                    // Auto-dispose
                }
            } catch (CommandCancelledException e) {
                OnCancelled(e);
            }

            OnCompleted();
        }

        private void OnStarted() {
            var eventArgs = new CommandTaskEventArgs(this);

            this.commandRunner.OnTaskStarted(eventArgs);

            var handler = Started;

            if (handler != null) {
                handler(this, eventArgs);
            }
        }

        private void OnProgressUpdated() {
            var eventArgs = new CommandTaskProgressEventArgs(this, this.progressTracker.Progress);

            this.commandRunner.OnProgressUpdated(eventArgs);

            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, eventArgs);
            }
        }

        private void OnCancelled(CommandCancelledException cancelReason) {
            lock (this.syncRoot) {
                this.cancellation = cancelReason;
                this.isCompleted = true;
            }

            var eventArgs = new CommandTaskCancellationEventArgs(this, cancelReason);

            this.commandRunner.OnTaskCancelled(eventArgs);

            var handler = Cancelled;

            if (handler != null) {
                handler(this, eventArgs);
            }
        }

        private void OnCompleted() {
            lock (this.syncRoot) {
                this.isCompleted = true;
            }

            var eventArgs = new CommandTaskEventArgs(this);

            this.commandRunner.OnTaskCompleted(eventArgs);

            var handler = Completed;

            if (handler != null) {
                handler(this, eventArgs);
            }
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

        public string Name {
            get {
                return this.command.Name;
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