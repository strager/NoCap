﻿using System;
using System.Collections.Generic;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    class DataRouterTimeEstimate : ITimeEstimate {
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

        public bool IsIndeterminant {
            get {
                throw new NotImplementedException();
            }
        }
    }

    [Serializable]
    public class DataRouter : ICommand {
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

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            ICommand command;

            if (!Routes.TryGetValue(data.DataType, out command)) {
                return null;
            }

            command.CheckValidInputType(data);

            return command.Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return Routes.Keys;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            ICommand command;

            if (!Routes.TryGetValue(input, out command)) {
                return null;
            }

            return command.GetOutputDataTypes(input);
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return this.timeEstimate;
            }
        }

        public void Connect(TypedDataType key, ICommand value) {
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
