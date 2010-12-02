using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NoCap.Library.Progress;
using NoCap.Library.Util;

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

    [Serializable]
    public sealed class DataRouter : ICommand {
        private readonly ITimeEstimate timeEstimate;
        private readonly IDictionary<TypedDataType, ICommand> routes;

        internal IDictionary<TypedDataType, ICommand> Routes {
            get {
                return this.routes;
            }
        }

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
                return null;
            }

            return command.Process(data, progress, cancelToken);
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return this.timeEstimate;
            }
        }

        public bool IsValid() {
            return this.routes.Values.All((command) => command.IsValidAndNotNull());
        }

        public void Connect(TypedDataType key, ICommand value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            Routes.Add(key, value);
        }
    }
}
