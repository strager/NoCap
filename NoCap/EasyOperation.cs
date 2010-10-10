using System;

namespace NoCap {
    public class EasyOperation : IOperation {
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
                    InvokeCompleted(new OperationResultEventArgs(this.Data));
                }
            }
        }

        public Exception Exception {
            get;
            set;
        }

        public TypedData Data {
            get;
            private set;
        }

        public Func<EasyOperation, TypedData> StartFunc {
            get;
            set;
        }

        public event EventHandler<OperationProgressEventArgs> ProgressUpdated;
        public event EventHandler<OperationResultEventArgs> Completed;

        public EasyOperation(Func<EasyOperation, TypedData> startFunc) {
            if (startFunc == null) {
                throw new ArgumentNullException("startFunc");
            }

            StartFunc = startFunc;
        }

        private void InvokeProgressUpdated(OperationProgressEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }

        private void InvokeCompleted(OperationResultEventArgs e) {
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

            var data = StartFunc(this);

            if (data != null) {
                Done(data);
            }
        }

        public void Cancel() {
            throw new NotImplementedException();
        }

        public void Done(TypedData data) {
            Data = data;
            State = OperationState.Completed;
        }
    }
}
