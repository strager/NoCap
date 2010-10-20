using System;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Destinations {
    public class ProcessorChain : IProcessor {
        private readonly List<IProcessor> destinations = new List<IProcessor>();

        public string Name {
            get { return "Destination chain"; }
        }

        public IList<IProcessor> Destinations {
            get {
                return this.destinations;
            }
        }

        public ProcessorChain() {
        }

        public ProcessorChain(params IProcessor[] processors) :
            this((IEnumerable<IProcessor>)processors) {
        }

        public ProcessorChain(IEnumerable<IProcessor> destinations) {
            this.destinations.AddRange(destinations);
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            // ToList is needed for some strange reason
            var progressTrackers = this.destinations.Select((destination) => new NotifyingProgressTracker()).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in Destinations) {
                    trackerEnumerator.MoveNext();
                    data = destination.Process(data, trackerEnumerator.Current);

                    if (data == null) {
                        // FIXME Maybe throw ...?
                        break;
                    }
                }
            }

            return data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            if (!this.destinations.Any()) {
                return new TypedDataType[] {
                };
            }

            return this.destinations.First().GetInputDataTypes();
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return this.destinations.Aggregate(
                (IEnumerable<TypedDataType>)new[] { input },
                (types, destination) => types.SelectMany((type) => destination.GetOutputDataTypes(type)).Unique()
            );
        }
    }
}
