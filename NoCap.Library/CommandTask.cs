using System;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library {
    public enum TaskState {
        NotStarted,
        Started,
        Running,
        Completed,
        Cancelled,
    }

    public sealed class CommandTask {
        private readonly object syncRoot = new object();

        private readonly ICommand command;
        private readonly CommandRunner commandRunner;

        private readonly IMutableProgressTracker progressTracker;
        private readonly IProgressTracker publicProgressTracker;

        private Thread thread;

        private CommandCancelledException cancelReason;
        private TaskState taskState = TaskState.NotStarted;

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

            this.progressTracker = new NotifyingProgressTracker();
            this.publicProgressTracker = new ReadOnlyProgressTracker(this.progressTracker);
        }

        internal void Run() {
            lock (this.syncRoot) {
                switch (TaskState) {
                    case TaskState.Started:
                        throw new InvalidOperationException("Task already started");

                    case TaskState.Running:
                        throw new InvalidOperationException("Task already running");

                    default:
                        throw new InvalidOperationException("Task already finished running");

                    case TaskState.NotStarted:
                        // Do nothing
                        break;
                }

                TaskState = TaskState.Started;

                this.thread = new Thread(RunThread);
                this.thread.Start();
            }
        }

        private void RunThread() {
            this.progressTracker.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    OnProgressUpdated();
                }
            };

            TaskState = TaskState.Running;
            OnStarted();

            try {
                using (command.Process(null, this.progressTracker)) {
                    // Auto-dispose
                }
            } catch (CommandCancelledException e) {
                TaskState = TaskState.Cancelled;
                OnCancelled(e);

                return;
            }

            TaskState = TaskState.Completed;
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

        private void OnCancelled(CommandCancelledException cancelException) {
            lock (this.syncRoot) {
                this.cancelReason = cancelException;
            }

            var eventArgs = new CommandTaskCancellationEventArgs(this, cancelException);

            this.commandRunner.OnTaskCancelled(eventArgs);

            var handler = Cancelled;

            if (handler != null) {
                handler(this, eventArgs);
            }
        }

        private void OnCompleted() {
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
                return TaskState == TaskState.Running;
            }
        }

        public bool IsCompleted {
            get {
                return TaskState == TaskState.Completed || TaskState == TaskState.Cancelled;
            }
        }

        public bool IsCancelled {
            get {
                return TaskState == TaskState.Cancelled;
            }
        }

        public CommandCancelledException CancelReason {
            get {
                lock (this.syncRoot) {
                    if (this.cancelReason == null) {
                        throw new InvalidOperationException("Task was not cancelled");
                    }

                    return this.cancelReason;
                }
            }
        }

        public string Name {
            get {
                return this.command.Name;
            }
        }

        private TaskState TaskState {
            get {
                lock (this.syncRoot) {
                    return this.taskState;
                }
            }

            set {
                lock (this.syncRoot) {
                    this.taskState = value;
                }
            }
        }

        public IProgressTracker ProgressTracker {
            get {
                if (TaskState == TaskState.NotStarted) {
                    throw new InvalidOperationException("Task not started");
                }

                return this.publicProgressTracker;
            }
        }

        public void WaitForCompletion() {
            if (IsCompleted) {
                return;
            }

            if (TaskState == TaskState.NotStarted) {
                throw new InvalidOperationException("Task not started");
            }

            this.thread.Join();
        }
    }
}