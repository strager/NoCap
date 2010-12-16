using System;
using System.Threading;

namespace NoCap.Library.Tasks {
    public class CommandRunner : ICommandRunner {
        public ICommandTask Run(ICommand command) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            var task = new CommandTask(command);
            SetUpTask(task);

            task.Run();

            return task;
        }

        public ICommandTask Run(ICommand command, CancellationTokenSource cancellationTokenSource) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            var task = new CommandTask(command, cancellationTokenSource);
            SetUpTask(task);

            task.Run();

            return task;
        }

        private void SetUpTask(ICommandTask task) {
            task.Started   += (sender, e) => OnTaskStarted  (e);
            task.Completed += (sender, e) => OnTaskCompleted(e);
            task.Canceled  += (sender, e) => OnTaskCanceled (e);
            task.ProgressTracker.ProgressUpdated += (sender, e) => OnProgressUpdated(new CommandTaskProgressEventArgs(task, e.Progress));
        }

        public event EventHandler<CommandTaskEventArgs> TaskStarted;
        public event EventHandler<CommandTaskEventArgs> TaskCompleted;
        public event EventHandler<CommandTaskCancellationEventArgs> TaskCanceled;
        public event EventHandler<CommandTaskProgressEventArgs> ProgressUpdated;

        protected virtual void OnTaskStarted(CommandTaskEventArgs e) {
            var handler = TaskStarted;

            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnTaskCompleted(CommandTaskEventArgs e) {
            var handler = TaskCompleted;

            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnTaskCanceled(CommandTaskCancellationEventArgs e) {
            var handler = TaskCanceled;

            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnProgressUpdated(CommandTaskProgressEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
