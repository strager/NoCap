using System;
using System.Threading;

namespace NoCap.Library.Tasks {
    public interface ICommandRunner {
        ICommandTask Run(ICommand command);
        ICommandTask Run(ICommand command, CancellationTokenSource cancellationTokenSource);

        event EventHandler<CommandTaskEventArgs> TaskStarted;
        event EventHandler<CommandTaskEventArgs> TaskCompleted;
        event EventHandler<CommandTaskCancellationEventArgs> TaskCanceled;
        event EventHandler<CommandTaskProgressEventArgs> ProgressUpdated;
    }
}