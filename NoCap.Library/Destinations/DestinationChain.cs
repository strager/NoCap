using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library.Destinations {
    public class DestinationChain : IDestination {
        private readonly List<IDestination> destinations = new List<IDestination>();

        public IList<IDestination> Destinations {
            get {
                return this.destinations;
            }
        }

        public DestinationChain() {
        }
        
        public DestinationChain(params IDestination[] destinations) :
            this((IEnumerable<IDestination>)destinations) {
        }

        public DestinationChain(IEnumerable<IDestination> destinations) {
            this.destinations.AddRange(destinations);
        }

        public TypedData Put(TypedData data, IMutableProgressTracker progress) {
            // ToList is needed for some strange reason
            var progressTrackers = this.destinations.Select((destination) => new NotifyingProgressTracker()).ToList();
            var aggregateProgress = new AggregateProgressTracker(progressTrackers);
            aggregateProgress.BindTo(progress);

            using (var trackerEnumerator = progressTrackers.GetEnumerator()) {
                foreach (var destination in Destinations) {
                    trackerEnumerator.MoveNext();
                    data = destination.Put(data, trackerEnumerator.Current);
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
