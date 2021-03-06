﻿using System;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Tasks {
    public sealed class CommandTask : ICommandTask {
        private readonly object syncRoot = new object();

        private readonly ICommand command;

        private readonly IMutableProgressTracker progressTracker;
        private readonly IProgressTracker publicProgressTracker;

        private Thread thread;
        private ManualResetEventSlim waitHandleOwner = new ManualResetEventSlim(false);

        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly bool ownsCancellationTokenSource;

        private CommandCanceledException cancelReason;
        private TaskState taskState = TaskState.NotStarted;

        public event EventHandler<CommandTaskEventArgs> Started;
        public event EventHandler<CommandTaskEventArgs> Completed;
        public event EventHandler<CommandTaskCancellationEventArgs> Canceled;

        public CommandTask(ICommand command) :
            this(command, new CancellationTokenSource(), true) {
        }

        public CommandTask(ICommand command, CancellationTokenSource cancellationTokenSource) :
            this(command, cancellationTokenSource, false) {
        }

        private CommandTask(ICommand command, CancellationTokenSource cancellationTokenSource, bool ownsCancellationTokenSource) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            if (cancellationTokenSource == null) {
                throw new ArgumentNullException("cancellationTokenSource");
            }

            this.command = command;
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

            State = TaskState.Running;
            OnStarted();

            try {
                try {
                    if (!command.IsValidAndNotNull()) {
                        throw new CommandInvalidException(command);
                    }

                    using (command.Process(null, this.progressTracker, cancelToken)) {
                        // Auto-dispose
                    }
                } catch (Exception e) {
                    HandleCancellation(e);

                    this.waitHandleOwner.Set();

                    return;
                }
            } finally {
                if (this.ownsCancellationTokenSource) {
                    this.cancellationTokenSource.Dispose();
                }
            }

            State = TaskState.Completed;
            OnCompleted();

            this.waitHandleOwner.Set();
        }

        private void HandleCancellation(Exception e) {
            State = TaskState.Canceled;

            var reason = CommandCanceledException.Wrap(e, this.command);

            lock (this.syncRoot) {
                this.cancelReason = reason;
            }

            this.progressTracker.Status = reason.Message;

            OnCanceled(reason);
        }

        private void OnStarted() {
            var handler = Started;

            if (handler != null) {
                handler(this, new CommandTaskEventArgs(this));
            }
        }

        private void OnCanceled(CommandCanceledException cancelException) {
            var handler = Canceled;

            if (handler != null) {
                handler(this, new CommandTaskCancellationEventArgs(this, cancelException));
            }
        }

        private void OnCompleted() {
            var handler = Completed;

            if (handler != null) {
                handler(this, new CommandTaskEventArgs(this));
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
                return this.publicProgressTracker;
            }
        }

        public WaitHandle WaitHandle {
            get {
                return this.waitHandleOwner.WaitHandle;
            }
        }
    }
}