using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NoCap.Library {
    public class CommandRunner {
        public CommandTask Run(ICommand command) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            var task = new CommandTask(command, this);
            task.Run();

            return task;
        }

        public event EventHandler<CommandTaskEventArgs> TaskStarted;
        public event EventHandler<CommandTaskEventArgs> TaskCompleted;
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

        internal protected virtual void OnProgressUpdated(CommandTaskProgressEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }
    }

    public class CommandTaskProgressEventArgs : EventArgs {
        private readonly CommandTask task;
        private readonly double progress;

        public CommandTaskProgressEventArgs(CommandTask task, double progress) {
            this.task = task;
            this.progress = progress;
        }

        public CommandTask Task {
            get {
                return this.task;
            }
        }

        public double Progress {
            get {
                return this.progress;
            }
        }
    }

    public class CommandTaskEventArgs : EventArgs {
        private readonly CommandTask task;

        public CommandTaskEventArgs(CommandTask task) {
            this.task = task;
        }

        public CommandTask Task {
            get {
                return this.task;
            }
        }
    }
}
