using System;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    [Serializable]
    public class CommandChain : ICommand {
        private readonly IList<ICommand> processors;

        public string Name {
            get { return "Destination chain"; }
        }

        public CommandChain() {
            this.processors = new List<ICommand>();
        }

        public CommandChain(params ICommand[] commands) :
            this((IEnumerable<ICommand>) commands) {
        }

        public CommandChain(IEnumerable<ICommand> processors) {
            this.processors = processors.ToList();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            // ToList is needed for some strange reason
            var progressTrackers = this.processors.Select((destination) => new NotifyingProgressTracker(destination.ProcessTimeEstimate)).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            bool shouldDisposeData = false;

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in this.processors) {
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
            if (!this.processors.Any()) {
                return new TypedDataType[] { };
            }

            return this.processors.First().GetInputDataTypes();
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            if (!this.processors.Any()) {
                return new[] { input };
            }

            this.CheckValidInputType(input);

            return GetChainOutputDataTypes(input, this.processors).Unique();
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

        public TimeEstimate ProcessTimeEstimate {
            get {
                int estimateCounter = this.processors.Aggregate(0, (counter, command) => (int) command.ProcessTimeEstimate);

                if (estimateCounter <= (int) TimeEstimate.NoTimeAtAll) {
                    return TimeEstimate.NoTimeAtAll;
                }

                if (estimateCounter <= (int) TimeEstimate.AShortWhile) {
                    return TimeEstimate.AShortWhile;
                }

                return TimeEstimate.Forever;
            }
        }

        public void Add(ICommand item) {
            this.processors.Add(item);
        }
    }
}
