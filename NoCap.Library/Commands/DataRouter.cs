using System;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    [Serializable]
    public class DataRouter : ICommand {
        private readonly IDictionary<TypedDataType, ICommand> routes;

        public string Name {
            get { return "Data router"; }
        }

        public DataRouter() {
            this.routes = new Dictionary<TypedDataType, ICommand>();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            ICommand command;

            if (!this.routes.TryGetValue(data.DataType, out command)) {
                return null;
            }

            command.CheckValidInputType(data);

            return command.Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return this.routes.Keys;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            ICommand command;

            if (!this.routes.TryGetValue(input, out command)) {
                return null;
            }

            return command.GetOutputDataTypes(input);
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                if (!this.routes.Any()) {
                    return TimeEstimates.Instantanious;
                }

                return this.routes.Values.Max((command) => command.ProcessTimeEstimate);
            }
        }

        public void Connect(TypedDataType key, ICommand value) {
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
