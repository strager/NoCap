using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    sealed class CommandChainTimeEstimate : ITimeEstimate {
        private readonly CommandChain commandChain;

        public CommandChainTimeEstimate(CommandChain commandChain) {
            this.commandChain = commandChain;
        }

        public double ProgressWeight {
            get {
                return this.commandChain.Commands.Sum((command) => command.ProcessTimeEstimate.ProgressWeight);
            }
        }

        public bool IsIndeterminate {
            get {
                return this.commandChain.Commands.Any((command) => command.ProcessTimeEstimate.IsIndeterminate);
            }
        }
    }

    [Serializable]
    public sealed class CommandChain : ICommand {
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

        public CommandChain() :
            this(Enumerable.Empty<ICommand>()) {
        }

        public CommandChain(params ICommand[] commands) :
            this((IEnumerable<ICommand>) commands) {
        }

        public CommandChain(IEnumerable<ICommand> commands) {
            this.commands = commands.ToList();

            this.timeEstimate = new CommandChainTimeEstimate(this);
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            // ToList is needed for some strange reason
            var progressTrackers = this.commands.Select((destination) => new NotifyingProgressTracker(destination.ProcessTimeEstimate)).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            bool shouldDisposeData = false;

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in this.commands) {
                    trackerEnumerator.MoveNext();

                    destination.CheckValidInputType(data);

                    var newData = destination.Process(data, trackerEnumerator.Current, cancelToken);

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

        public ICommandFactory GetFactory() {
            return null;
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return this.timeEstimate;
            }
        }

        public bool IsValid() {
            return this.commands.All((command) => command.IsValidAndNotNull());
        }

        public void Add(ICommand item) {
            this.commands.Add(item);
        }
    }
}
