using System;
using NoCap.Library.Util;

namespace NoCap.Library.Tasks {
    public enum TaskState {
        NotStarted,
        Started,
        Running,
        Completed,
        Canceled,
    }

    public interface ICommandTask {
        event EventHandler<CommandTaskEventArgs> Started;
        event EventHandler<CommandTaskEventArgs> Completed;
        event EventHandler<CommandTaskCancellationEventArgs> Canceled;
        event EventHandler<CommandTaskProgressEventArgs> ProgressUpdated;

        string Name {
            get;
        }

        ICommand Command {
            get;
        }

        CommandCanceledException CancelReason {
            get;
        }

        TaskState State {
            get;
        }

        IProgressTracker ProgressTracker {
            get;
        }

        void Cancel();
        void WaitForCompletion();
    }

    public class CommandTaskCancellationEventArgs : CommandTaskEventArgs {
        private readonly CommandCanceledException cancelReason;

        public CommandTaskCancellationEventArgs(ICommandTask task, CommandCanceledException cancelReason) :
            base(task) {
            this.cancelReason = cancelReason;
        }

        public CommandCanceledException CancelReason {
            get {
                return this.cancelReason;
            }
        }
    }

    public class CommandTaskProgressEventArgs : CommandTaskEventArgs {
        private readonly double progress;

        public CommandTaskProgressEventArgs(ICommandTask task, double progress) :
            base(task) {
            this.progress = progress;
        }

        public double Progress {
            get {
                return this.progress;
            }
        }
    }

    public class CommandTaskEventArgs : EventArgs {
        private readonly ICommandTask task;

        public CommandTaskEventArgs(ICommandTask task) {
            this.task = task;
        }

        public ICommandTask Task {
            get {
                return this.task;
            }
        }
    }
}