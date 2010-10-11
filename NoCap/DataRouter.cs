using System.Collections.Generic;
using NoCap.Destinations;

namespace NoCap {
    public class DataRouter : IDestination {
        private readonly IDictionary<TypedDataType, IDestination> routes = new Dictionary<TypedDataType, IDestination>();

        public IDictionary<TypedDataType, IDestination> Routes {
            get {
                return routes;
            }
        }

        public IOperation<TypedData> Put(TypedData data) {
            IDestination destination;

            if (!routes.TryGetValue(data.Type, out destination)) {
                return null;
            }

            if (destination == null) {
                return null;
            }

            return destination.Put(data);
        }
    }
}
