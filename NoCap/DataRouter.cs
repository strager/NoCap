using System.Collections.Generic;
using NoCap.Destinations;

namespace NoCap {
    public class DataRouter : IDestination {
        private readonly IDictionary<DestinationType, IDestination> routes = new Dictionary<DestinationType, IDestination>();

        public IDictionary<DestinationType, IDestination> Routes {
            get {
                return routes;
            }
        }

        public bool Put(DestinationType type, object data, string name, IResultThing result) {
            IDestination destination;

            if (!routes.TryGetValue(type , out destination)) {
                return false;
            }

            if (destination == null) {
                return false;
            }

            return destination.Put(type, data, name, result);
        }
    }
}
