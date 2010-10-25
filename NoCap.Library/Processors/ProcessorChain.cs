using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Processors {
    public class ProcessorChain : IProcessor {
        private readonly IList<IProcessor> processors;

        public string Name {
            get { return "Destination chain"; }
        }

        public ProcessorChain() {
            this.processors = new List<IProcessor>();
        }

        public ProcessorChain(params IProcessor[] processors) :
            this((IEnumerable<IProcessor>) processors) {
        }

        public ProcessorChain(IEnumerable<IProcessor> processors) {
            this.processors = processors.ToList();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            // ToList is needed for some strange reason
            var progressTrackers = this.processors.Select((destination) => new NotifyingProgressTracker()).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in this.processors) {
                    trackerEnumerator.MoveNext();

                    destination.CheckValidInputType(data);

                    data = destination.Process(data, trackerEnumerator.Current);
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

        private static IEnumerable<TypedDataType> GetChainOutputDataTypes(TypedDataType input, IEnumerable<IProcessor> processors) {
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

        public IProcessorFactory GetFactory() {
            return null;
        }

        public void Add(IProcessor item) {
            this.processors.Add(item);
        }
    }
}
