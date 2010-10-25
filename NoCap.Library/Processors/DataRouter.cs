using System;
using System.Collections.Generic;
using NoCap.Library.Util;

namespace NoCap.Library.Processors {
    public class DataRouter : IProcessor {
        public IDictionary<TypedDataType, IProcessor> Routes {
            get;
            private set;
        }

        public string Name {
            get { return "Data router"; }
        }

        public DataRouter() {
            Routes = new Dictionary<TypedDataType, IProcessor>();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            IProcessor processor;

            if (!Routes.TryGetValue(data.DataType, out processor)) {
                return null;
            }

            processor.CheckValidInputType(data);

            return processor.Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return Routes.Keys;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            IProcessor processor;

            if (!Routes.TryGetValue(input, out processor)) {
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

            Routes.Add(key, value);
        }
    }
}
