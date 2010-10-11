using System;
using System.Collections.Generic;

namespace NoCap.Library.Destinations {
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
                throw new InvalidOperationException("Routed destination must not be null");
            }

            return destination.Put(data);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return this.Routes.Keys;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            IDestination destination;

            if (!routes.TryGetValue(input, out destination)) {
                return null;
            }

            return destination.GetOutputDataTypes(input);
        }
    }
}
