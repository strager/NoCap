using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Commands {
    sealed class DataRouterTimeEstimate : ITimeEstimate {
        private readonly DataRouter dataRouter;

        public DataRouterTimeEstimate(DataRouter dataRouter) {
            this.dataRouter = dataRouter;
        }

        public double ProgressWeight {
            get {
                if (!this.dataRouter.Routes.Values.Any()) {
                    return 0;
                }

                return this.dataRouter.Routes.Values.Max((command) => command.ProcessTimeEstimate.ProgressWeight);
            }
        }

        public bool IsIndeterminate {
            get {
                throw new NotImplementedException();
            }
        }
    }

    [DataContract(Name = "DataRouter")]
    public sealed class DataRouter : IEnumerable, ICommand, IExtensibleDataObject {
        private readonly ITimeEstimate timeEstimate;
        private readonly IDictionary<TypedDataType, ICommand> routes;

        [DataMember(Name = "Routes")]
        internal IDictionary<TypedDataType, ICommand> Routes {
            get {
                return this.routes;
            }
        }

        [IgnoreDataMember]
        public string Name {
            get { return "Data router"; }
        }

        public DataRouter() {
            this.timeEstimate = new DataRouterTimeEstimate(this);
            this.routes = new Dictionary<TypedDataType, ICommand>();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            ICommand command;

            if (!Routes.TryGetValue(data.DataType, out command)) {
                throw new InvalidOperationException("Data type not routed");
            }

            return command.Process(data, progress, cancelToken);
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        [IgnoreDataMember]
        public ITimeEstimate ProcessTimeEstimate {
            get {
                return this.timeEstimate;
            }
        }

        public bool IsValid() {
            return true;
        }

        public void Add(TypedDataType key, ICommand value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            Routes.Add(key, value);
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }

        public IEnumerator GetEnumerator() {
            return this.routes.GetEnumerator();
        }
    }
}
