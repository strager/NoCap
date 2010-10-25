using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Processors {
    public class ProcessorChain : IProcessor {
        public IList<IProcessor> Processors {
            get;
            set;
        }

        public string Name {
            get { return "Destination chain"; }
        }

        public ProcessorChain() {
            Processors = new List<IProcessor>();
        }

        public ProcessorChain(params IProcessor[] processors) :
            this((IEnumerable<IProcessor>) processors) {
        }

        public ProcessorChain(IEnumerable<IProcessor> processors) {
            Processors = processors.ToList();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            // ToList is needed for some strange reason
            var progressTrackers = Processors.Select((destination) => new NotifyingProgressTracker()).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in Processors) {
                    trackerEnumerator.MoveNext();

                    destination.CheckValidInputType(data);

                    data = destination.Process(data, trackerEnumerator.Current);
                }
            }

            return data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            if (!Processors.Any()) {
                return new TypedDataType[] { };
            }

            return Processors.First().GetInputDataTypes();
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            if (!Processors.Any()) {
                return new[] { input };
            }

            this.CheckValidInputType(input);

            return GetChainOutputDataTypes(input, Processors).Unique();
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
            Processors.Add(item);
        }
    }
}
