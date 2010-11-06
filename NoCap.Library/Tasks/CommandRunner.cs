using System;

namespace NoCap.Library.Tasks {
    public class CommandRunner {
        public ICommandTask Run(ICommand command) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            var task = new CommandTask(command, this);
            task.Run();

            return task;
        }

        public event EventHandler<CommandTaskEventArgs> TaskStarted;
        public event EventHandler<CommandTaskEventArgs> TaskCompleted;
        public event EventHandler<CommandTaskCancellationEventArgs> TaskCancelled;
        public event EventHandler<CommandTaskProgressEventArgs> ProgressUpdated;

        internal protected virtual void OnTaskStarted(CommandTaskEventArgs e) {
            var handler = TaskStarted;

            if (handler != null) {
                handler(this, e);
            }
        }

        internal protected virtual void OnTaskCompleted(CommandTaskEventArgs e) {
            var handler = TaskCompleted;

            if (handler != null) {
                handler(this, e);
            }
        }

        internal protected virtual void OnTaskCancelled(CommandTaskCancellationEventArgs e) {
            var handler = TaskCancelled;

            if (handler != null) {
                handler(this, e);
            }
        }

        internal protected virtual void OnProgressUpdated(CommandTaskProgressEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
