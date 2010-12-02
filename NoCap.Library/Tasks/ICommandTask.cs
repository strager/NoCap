using System;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Tasks {
    /// <summary>
    /// Represents the state which an <see cref="ICommandTask"/> is currently in.
    /// </summary>
    public enum TaskState {
        /// <summary>The task has not yet started.</summary>
        NotStarted,

        /// <summary>The task has started but <see cref="ICommand.Process"/> has not yet been called.</summary>
        Started,

        /// <summary>The task has started and <see cref="ICommand.Process"/> has been called.</summary>
        Running,

        /// <summary><see cref="ICommand.Process"/> has returned without fail.</summary>
        Completed,

        /// <summary>The task has been canceled.</summary>
        Canceled,
    }

    /// <summary>
    /// Represents the execution of an <see cref="ICommand"/>.
    /// </summary>
    public interface ICommandTask {
        /// <summary>
        /// Occurs when <see cref="Command"/> begins execution.
        /// </summary>
        event EventHandler<CommandTaskEventArgs> Started;

        /// <summary>
        /// Occurs when <see cref="Command"/> ends execution successfully.
        /// </summary>
        event EventHandler<CommandTaskEventArgs> Completed;

        /// <summary>
        /// Occurs when <see cref="Command"/> ends execution through cancellation.
        /// </summary>
        event EventHandler<CommandTaskCancellationEventArgs> Canceled;

        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        string Name { get; }

        /// <summary>
        /// Gets the command being executed by the task.
        /// </summary>
        /// <value>The command.</value>
        ICommand Command { get; }

        /// <summary>
        /// Gets the reason the task was cancelled.
        /// </summary>
        /// <value>The reason for cancellation.</value>
        /// <exception cref="InvalidOperationException">The task was not cancelled.</exception>
        CommandCanceledException CancelReason { get; }

        /// <summary>
        /// Gets the current state of the task.
        /// </summary>
        /// <value>The current task state.</value>
        TaskState State { get; }

        /// <summary>
        /// Gets the progress tracker of the task.
        /// </summary>
        /// <value>The progress tracker.</value>
        IProgressTracker ProgressTracker { get; }

        /// <summary>
        /// Gets the wait handle for competion or cancellation of the task.
        /// </summary>
        /// <value>The wait handle.</value>
        WaitHandle WaitHandle { get; }

        /// <summary>
        /// Requests the task to be cancelled.
        /// </summary>
        /// <remarks>
        /// Because tasks are asynchronous, the task may not be cancelled immediately.
        /// In addition, the task can be marked as completed before the cancellation
        /// takes place.  Use the <see cref="Completed"/> event to be notified when a
        /// cancellation actually happens.
        /// </remarks>
        void Cancel();
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