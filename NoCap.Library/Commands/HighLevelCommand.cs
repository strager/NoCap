using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Commands {
    [DataContract(Name = "HighLevelCommand")]
    public abstract class HighLevelCommand : ICommand {
        public abstract string Name { get; }

        TypedData ICommand.Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            Execute(progress, cancelToken);

            return null;
        }

        public abstract ICommandFactory GetFactory();

        public abstract ITimeEstimate ProcessTimeEstimate { get; }

        public abstract bool IsValid();

        public abstract void Execute(IMutableProgressTracker progress, CancellationToken cancelToken);
    }
}
