using System;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library {
    public class CommandTask {
        private readonly object syncRoot = new object();

        private readonly ICommand command;
        private readonly CommandRunner commandRunner;
        private IMutableProgressTracker progressTracker;
        private Thread thread;

        private bool isCompleted;

        internal CommandTask(ICommand command, CommandRunner commandRunner) {
            this.command = command;
            this.commandRunner = commandRunner;
        }

        internal void Run() {
            lock (this.syncRoot) {
                if (IsRunning) {
                    throw new InvalidOperationException("Task already running");
                }

                this.isCompleted = false;

                this.commandRunner.OnTaskStarted(new CommandTaskEventArgs(this));

                this.thread = new Thread(RunThread);
                this.thread.Start();
            }
        }

        private void RunThread() {
            this.progressTracker = new NotifyingProgressTracker();
            this.progressTracker.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    this.commandRunner.OnProgressUpdated(new CommandTaskProgressEventArgs(this, this.progressTracker.Progress));
                }
            };

            command.Process(null, this.progressTracker);

            this.commandRunner.OnTaskCompleted(new CommandTaskEventArgs(this));

            lock (this.syncRoot) {
                this.isCompleted = true;
            }
        }

        public ICommand Command {
            get {
                return this.command;
            }
        }

        public bool IsRunning {
            get {
                return this.thread != null && this.thread.IsAlive;
            }
        }

        public bool IsCompleted {
            get {
                return this.isCompleted;
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