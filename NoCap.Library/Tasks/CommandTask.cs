using System;
using System.Threading;
using NoCap.Library.Progress;
using NoCap.Library.Util;

namespace NoCap.Library.Tasks {
    sealed class CommandTask : ICommandTask {
        private readonly object syncRoot = new object();

        private readonly ICommand command;
        private readonly CommandRunner commandRunner;

        private readonly IMutableProgressTracker progressTracker;
        private readonly IProgressTracker publicProgressTracker;

        private Thread thread;

        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly bool ownsCancellationTokenSource;

        private CommandCanceledException cancelReason;
        private TaskState taskState = TaskState.NotStarted;

        public event EventHandler<CommandTaskEventArgs> Started;
        public event EventHandler<CommandTaskEventArgs> Completed;
        public event EventHandler<CommandTaskCancellationEventArgs> Canceled;
        public event EventHandler<CommandTaskProgressEventArgs> ProgressUpdated;

        public CommandTask(ICommand command, CommandRunner commandRunner) :
            this(command, commandRunner, new CancellationTokenSource(), true) {
        }

        public CommandTask(ICommand command, CommandRunner commandRunner, CancellationTokenSource cancellationTokenSource) :
            this(command, commandRunner, cancellationTokenSource, false) {
        }

        private CommandTask(ICommand command, CommandRunner commandRunner, CancellationTokenSource cancellationTokenSource, bool ownsCancellationTokenSource) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            if (commandRunner == null) {
                throw new ArgumentNullException("commandRunner");
            }

            if (cancellationTokenSource == null) {
                throw new ArgumentNullException("cancellationTokenSource");
            }

            this.command = command;
            this.commandRunner = commandRunner;
            this.cancellationTokenSource = cancellationTokenSource;
            this.ownsCancellationTokenSource = ownsCancellationTokenSource;

            this.progressTracker = new MutableProgressTracker();
            this.publicProgressTracker = new ReadOnlyProgressTracker(this.progressTracker);
        }

        public void Run() {
            lock (this.syncRoot) {
                switch (State) {
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

                State = TaskState.Started;

                this.thread = new Thread(RunThread);
                this.thread.Start();
            }
        }

        private void RunThread() {
            var cancelToken = this.cancellationTokenSource == null
                ? CancellationToken.None
                : this.cancellationTokenSource.Token;

            this.progressTracker.ProgressUpdated += (sender, e) => OnProgressUpdated();

            State = TaskState.Running;
            OnStarted();

            try {
                try {
                    using (command.Process(null, this.progressTracker, cancelToken)) {
                        // Auto-dispose
                    }
                } catch (OperationCanceledException e) {
                    State = TaskState.Canceled;
                    OnCanceled(CommandCanceledException.Wrap(e, command));

                    return;
                }
            } finally {
                if (this.ownsCancellationTokenSource) {
                    this.cancellationTokenSource.Dispose();
                }
            }

            State = TaskState.Completed;
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

        private void OnCanceled(CommandCanceledException cancelException) {
            lock (this.syncRoot) {
                this.cancelReason = cancelException;
            }

            var eventArgs = new CommandTaskCancellationEventArgs(this, cancelException);

            this.commandRunner.OnTaskCanceled(eventArgs);

            var handler = Canceled;

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

        public void Cancel() {
            switch (State) {
                case TaskState.Started:
                case TaskState.Running:
                    this.cancellationTokenSource.Cancel();

                    break;
            }
        }

        public string Name {
            get {
                return this.command.Name;
            }
        }

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public CommandCanceledException CancelReason {
            get {
                lock (this.syncRoot) {
                    if (this.cancelReason == null) {
                        throw new InvalidOperationException("Task was not cancelled");
                    }

                    return this.cancelReason;
                }
            }
        }

        public TaskState State {
            get {
                lock (this.syncRoot) {
                    return this.taskState;
                }
            }

            private set {
                lock (this.syncRoot) {
                    this.taskState = value;
                }
            }
        }

        public IProgressTracker ProgressTracker {
            get {
                if (State == TaskState.NotStarted) {
                    throw new InvalidOperationException("Task not started");
                }

                return this.publicProgressTracker;
            }
        }

        public void WaitForCompletion() {
            switch (State) {
                case TaskState.Canceled:
                case TaskState.Completed:
                    return;

                case TaskState.NotStarted:
                    throw new InvalidOperationException("Task not started");

                default:
                    this.thread.Join();

                    break;
            }
        }
    }
}