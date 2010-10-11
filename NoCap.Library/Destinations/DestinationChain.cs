using System;
using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library.Destinations {
    public class DestinationChain : IDestination {
        private readonly List<IDestination> destinations = new List<IDestination>();

        public IList<IDestination> Destinations {
            get {
                return this.destinations;
            }
        }

        public DestinationChain() {
        }

        public DestinationChain(IEnumerable<IDestination> destinations) {
            this.destinations.AddRange(destinations);
        }

        public IOperation<TypedData> Put(TypedData data) {
            return new DestinationChainOperation(this.destinations, data);
        }
    }

    class DestinationChainOperation : IOperation<TypedData> {
        private readonly IEnumerable<IDestination> destinations;

        private double progress;
        private OperationState state;

        public double Progress {
            get {
                return this.progress;
            }

            private set {
                this.progress = value;

                InvokeProgressUpdated(new OperationProgressEventArgs(this.progress));
            }
        }

        public OperationState State {
            get {
                return this.state;
            }

            private set {
                this.state = value;

                if (this.state == OperationState.Completed) {
                    InvokeCompleted(new OperationResultEventArgs<TypedData>(Data));
                }
            }
        }

        public Exception Exception {
            get;
            private set;
        }

        public TypedData Data {
            get;
            private set;
        }

        private TypedData currentData;

        public DestinationChainOperation(IEnumerable<IDestination> destinations, TypedData data) {
            this.destinations = destinations;
            this.currentData = data;
        }

        public event EventHandler<OperationProgressEventArgs> ProgressUpdated;
        public event EventHandler<OperationResultEventArgs<TypedData>> Completed;

        private void InvokeProgressUpdated(OperationProgressEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }

        private void InvokeCompleted(OperationResultEventArgs<TypedData> e) {
            var handler = Completed;

            if (handler != null) {
                handler(this, e);
            }
        }

        public void Start() {
            State = OperationState.Started;

            Next(this.destinations);
        }

        private void Next(IEnumerable<IDestination> destinations) {
            if (!destinations.Any()) {
                Data = this.currentData;
                State = OperationState.Completed;

                return;
            }

            var op = destinations.First().Put(this.currentData);

            op.Completed += (sender, e) => {
                this.currentData = e.Data;
                Next(destinations.Skip(1));
            };

            // TODO Error handling
            // TODO Progress tracking

            op.Start();
        }

        public void Cancel() {
            throw new NotImplementedException();
        }
    }
}
