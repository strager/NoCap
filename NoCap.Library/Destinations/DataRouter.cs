using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NoCap.Library.Sources;

namespace NoCap.Library.Destinations {
    public class DataRouter : IDestination, IDictionary<TypedDataType, IDestination> {
        private readonly IDictionary<TypedDataType, IDestination> routes = new Dictionary<TypedDataType, IDestination>();

        public TypedData Put(TypedData data, IProgressTracker progress) {
            IDestination destination;

            if (!routes.TryGetValue(data.DataType, out destination)) {
                return null;
            }

            return destination.Put(data, progress);
        }

        public TypedData RouteFrom(ISource source, IProgressTracker progress) {
            // TODO AggregateProgress thingy
            var data = source.Get(progress);

            if (data == null) {
                return null;
            }

            data = Put(data, progress);

            return data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return this.routes.Keys;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            IDestination destination;

            if (!routes.TryGetValue(input, out destination)) {
                return null;
            }

            return destination.GetOutputDataTypes(input);
        }

        public bool CanRoute(TypedDataType type, IDestination destination) {
            return destination.GetInputDataTypes().Contains(type);
        }

        public IEnumerator<KeyValuePair<TypedDataType, IDestination>> GetEnumerator() {
            return this.routes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.routes.GetEnumerator();
        }

        public void Add(KeyValuePair<TypedDataType, IDestination> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
            this.routes.Clear();
        }

        public bool Contains(KeyValuePair<TypedDataType, IDestination> item) {
            return this.routes.Contains(item);
        }

        public void CopyTo(KeyValuePair<TypedDataType, IDestination>[] array, int arrayIndex) {
            this.routes.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TypedDataType, IDestination> item) {
            return this.routes.Remove(item);
        }

        public int Count {
            get {
                return this.routes.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public bool ContainsKey(TypedDataType key) {
            return this.routes.ContainsKey(key);
        }

        public void Add(TypedDataType key, IDestination value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            if (!CanRoute(key, value)) {
                throw new ArgumentException("Destination must accept type", "value");
            }

            this.routes.Add(key, value);
        }

        public bool Remove(TypedDataType key) {
            return this.routes.Remove(key);
        }

        public bool TryGetValue(TypedDataType key, out IDestination value) {
            return this.routes.TryGetValue(key, out value);
        }

        public IDestination this[TypedDataType key] {
            get {
                return this.routes[key];
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                if (!CanRoute(key, value)) {
                    throw new ArgumentException("Destination must accept type", "value");
                }

                this.routes[key] = value;
            }
        }

        public ICollection<TypedDataType> Keys {
            get {
                return this.routes.Keys;
            }
        }

        public ICollection<IDestination> Values {
            get {
                return this.routes.Values;
            }
        }
    }
}
