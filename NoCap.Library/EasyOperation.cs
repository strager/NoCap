using System;

namespace NoCap {
    public class EasyOperation<T> : IOperation<T> {
        private double progress;
        private OperationState state;

        public double Progress {
            get {
                return this.progress;
            }

            set {
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
                    InvokeCompleted(new OperationResultEventArgs<T>(Data));
                }
            }
        }

        public Exception Exception {
            get;
            set;
        }

        public T Data {
            get;
            private set;
        }

        public Func<EasyOperation<T>, T> StartFunc {
            get;
            set;
        }

        public event EventHandler<OperationProgressEventArgs> ProgressUpdated;
        public event EventHandler<OperationResultEventArgs<T>> Completed;

        public EasyOperation(Func<EasyOperation<T>, T> startFunc = null) {
            StartFunc = startFunc;
        }

        private void InvokeProgressUpdated(OperationProgressEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }

        private void InvokeCompleted(OperationResultEventArgs<T> e) {
            var handler = Completed;

            if (handler != null) {
                handler(this, e);
            }
        }

        public void Start() {
            if (State != OperationState.NotYetStarted) {
                throw new InvalidOperationException();
            }

            State = OperationState.Started;

            if (StartFunc == null) {
                return;
            }

            var data = StartFunc(this);

            if (data != null) {
                Done(data);
            }
        }

        public void Cancel() {
            throw new NotImplementedException();
        }

        public void Done(T data) {
            Data = data;
            State = OperationState.Completed;
        }
    }
}
