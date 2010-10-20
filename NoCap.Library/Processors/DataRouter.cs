using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library.Processors {
    public class DataRouter : IProcessor, IDictionary<TypedDataType, IProcessor> {
        private readonly IDictionary<TypedDataType, IProcessor> routes = new Dictionary<TypedDataType, IProcessor>();

        public string Name {
            get { return "Data router"; }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            IProcessor processor;

            if (!routes.TryGetValue(data.DataType, out processor)) {
                return null;
            }

            return processor.Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return this.routes.Keys;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            IProcessor processor;

            if (!routes.TryGetValue(input, out processor)) {
                return null;
            }

            return processor.GetOutputDataTypes(input);
        }

        public IProcessorFactory GetFactory() {
            return null;
        }

        public bool CanRoute(TypedDataType type, IProcessor processor) {
            return processor.GetInputDataTypes().Contains(type);
        }

        public IEnumerator<KeyValuePair<TypedDataType, IProcessor>> GetEnumerator() {
            return this.routes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.routes.GetEnumerator();
        }

        public void Add(KeyValuePair<TypedDataType, IProcessor> item) {
            Add(item.Key, item.Value);
        }

        public void Clear() {
            this.routes.Clear();
        }

        public bool Contains(KeyValuePair<TypedDataType, IProcessor> item) {
            return this.routes.Contains(item);
        }

        public void CopyTo(KeyValuePair<TypedDataType, IProcessor>[] array, int arrayIndex) {
            this.routes.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TypedDataType, IProcessor> item) {
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

        public void Add(TypedDataType key, IProcessor value) {
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

        public bool TryGetValue(TypedDataType key, out IProcessor value) {
            return this.routes.TryGetValue(key, out value);
        }

        public IProcessor this[TypedDataType key] {
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

        public ICollection<IProcessor> Values {
            get {
                return this.routes.Values;
            }
        }
    }
}
