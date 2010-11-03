using System;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    class CommandChainTimeEstimate : ITimeEstimate {
        private readonly CommandChain commandChain;

        public CommandChainTimeEstimate(CommandChain commandChain) {
            this.commandChain = commandChain;
        }

        public double ProgressWeight {
            get {
                return this.commandChain.Commands.Sum((command) => command.ProcessTimeEstimate.ProgressWeight);
            }
        }

        public bool IsIndeterminant {
            get {
                return this.commandChain.Commands.Any((command) => command.ProcessTimeEstimate.IsIndeterminant);
            }
        }
    }

    [Serializable]
    public class CommandChain : ICommand {
        private readonly ITimeEstimate timeEstimate;
        private readonly IList<ICommand> commands;

        public string Name {
            get { return "Destination chain"; }
        }

        internal IEnumerable<ICommand> Commands {
            get {
                return this.commands;
            }
        }

        public CommandChain() {
            this.commands = new List<ICommand>();
        }

        public CommandChain(params ICommand[] commands) :
            this((IEnumerable<ICommand>) commands) {
        }

        public CommandChain(IEnumerable<ICommand> commands) {
            this.commands = commands.ToList();

            this.timeEstimate = new CommandChainTimeEstimate(this);
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            // ToList is needed for some strange reason
            var progressTrackers = this.commands.Select((destination) => new NotifyingProgressTracker(destination.ProcessTimeEstimate)).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            bool shouldDisposeData = false;

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in this.commands) {
                    trackerEnumerator.MoveNext();

                    destination.CheckValidInputType(data);

                    var newData = destination.Process(data, trackerEnumerator.Current);

                    if (shouldDisposeData && data != null) {
                        data.Dispose();
                    }

                    data = newData;
                    shouldDisposeData = true;
                }
            }

            return data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            if (!this.commands.Any()) {
                return new TypedDataType[] { };
            }

            return this.commands.First().GetInputDataTypes();
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            if (!this.commands.Any()) {
                return new[] { input };
            }

            this.CheckValidInputType(input);

            return GetChainOutputDataTypes(input, this.commands).Unique();
        }

        private static IEnumerable<TypedDataType> GetChainOutputDataTypes(TypedDataType input, IEnumerable<ICommand> processors) {
            if (!processors.Any()) {
                return new[] { input };
            }

            var processor = processors.First();

            if (!processor.IsValidInputType(input)) {
                return Enumerable.Empty<TypedDataType>();
            }

            var outputTypes = processor.GetOutputDataTypes(input);

            return outputTypes.Aggregate(
                Enumerable.Empty<TypedDataType>(),
                (types, type) => types.Concat(GetChainOutputDataTypes(type, processors.Skip(1)))
            );
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return this.timeEstimate;
            }
        }

        public void Add(ICommand item) {
            this.commands.Add(item);
        }
    }
}
