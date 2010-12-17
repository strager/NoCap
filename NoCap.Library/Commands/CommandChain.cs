using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

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

    [DataContract(Name = "CommandChain")]
    public sealed class CommandChain : ICommand, IExtensibleDataObject {
        private readonly ITimeEstimate timeEstimate;
        private readonly IEnumerable<ICommand> commands;

        [IgnoreDataMember]
        public string Name {
            get { return "Destination chain"; }
        }

        [DataMember(Name = "Commands")]
        internal IEnumerable<ICommand> Commands {
            get {
                return this.commands;
            }
        }

        public CommandChain(params ICommand[] commands) :
            this((IEnumerable<ICommand>) commands) {
        }

        public CommandChain(IEnumerable<ICommand> commands) {
            this.commands = commands.ToArray(); // Make a copy

            this.timeEstimate = new CommandChainTimeEstimate(this);
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            // ToList is needed to prevent new MPT's from being
            // created for each loop of commandProgressTrackers
            var commandProgressTrackers = this.commands.Select((command) => new {
                ProgressTracker = new MutableProgressTracker(),
                Command = command
            }).ToList();

            var aggregateProgress = new AggregateProgressTracker(commandProgressTrackers.Select(
                (cpt) => new ProgressTrackerCollectionItem(
                    cpt.ProgressTracker,
                    cpt.Command.ProcessTimeEstimate.ProgressWeight
                )
            ));

            aggregateProgress.BindTo(progress);

            bool shouldDisposeData = false;

            foreach (var cpt in commandProgressTrackers) {
                cancelToken.ThrowIfCancellationRequested();

                TypedData newData;

                try {
                    cancelToken.ThrowIfCancellationRequested();

                    newData = cpt.Command.Process(data, cpt.ProgressTracker, cancelToken);

                    cancelToken.ThrowIfCancellationRequested();
                } finally {
                    if (shouldDisposeData && data != null) {
                        data.Dispose();
                    }
                }

                data = newData;
                shouldDisposeData = true;
            }

            return data;
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        [IgnoreDataMember]
        public ITimeEstimate ProcessTimeEstimate {
            get {
                return this.timeEstimate;
            }
        }

        public bool IsValid() {
            return this.commands.All((command) => command.IsValidAndNotNull());
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}
