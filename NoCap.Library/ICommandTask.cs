using System;
using NoCap.Library.Util;

namespace NoCap.Library {
    public enum TaskState {
        NotStarted,
        Started,
        Running,
        Completed,
        Cancelled,
    }

    public interface ICommandTask {
        event EventHandler<CommandTaskEventArgs> Started;
        event EventHandler<CommandTaskEventArgs> Completed;
        event EventHandler<CommandTaskCancellationEventArgs> Cancelled;
        event EventHandler<CommandTaskProgressEventArgs> ProgressUpdated;

        string Name {
            get;
        }

        ICommand Command {
            get;
        }

        CommandCancelledException CancelReason {
            get;
        }

        TaskState State {
            get;
        }

        IProgressTracker ProgressTracker {
            get;
        }

        void WaitForCompletion();
    }

    public class CommandTaskCancellationEventArgs : CommandTaskEventArgs {
        private readonly CommandCancelledException cancelReason;

        public CommandTaskCancellationEventArgs(ICommandTask task, CommandCancelledException cancelReason) :
            base(task) {
            this.cancelReason = cancelReason;
        }

        public CommandCancelledException CancelReason {
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