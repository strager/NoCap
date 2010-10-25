using System;
using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library.Processors {
    public class DataRouter : IProcessor {
        private readonly IDictionary<TypedDataType, IProcessor> routes;

        public string Name {
            get { return "Data router"; }
        }

        public DataRouter() {
            this.routes = new Dictionary<TypedDataType, IProcessor>();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            IProcessor processor;

            if (!this.routes.TryGetValue(data.DataType, out processor)) {
                return null;
            }

            processor.CheckValidInputType(data);

            return processor.Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return this.routes.Keys;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            IProcessor processor;

            if (!this.routes.TryGetValue(input, out processor)) {
                return null;
            }

            return processor.GetOutputDataTypes(input);
        }

        public IProcessorFactory GetFactory() {
            return null;
        }

        public void Route(TypedDataType key, IProcessor value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            if (!value.IsValidInputType(key)) {
                throw new ArgumentException("Destination must accept type", "value");
            }

            this.routes.Add(key, value);
        }
    }
}
