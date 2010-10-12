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

        public TypedData Put(TypedData data, IProgressTracker progress) {
            // TODO AggregateProgressable thingy

            return this.destinations.Aggregate(data, (current, destination) => destination.Put(current, progress));
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
