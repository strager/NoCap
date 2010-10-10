using System;

namespace NoCap {
    public interface IOperation {
        double Progress { get; }
        OperationState State { get; }

        Exception Exception { get; }

        TypedData Data { get; }

        event EventHandler<OperationProgressEventArgs> ProgressUpdated;
        event EventHandler<OperationResultEventArgs> Completed;

        void Start();
        void Cancel();
    }

    public enum OperationState {
        NotYetStarted = 0,  // TODO Better name
        Started,
        Completed,
        Cancelled
    }

    public class OperationResultEventArgs : EventArgs {
        private readonly TypedData data;

        public TypedData Data {
            get {
                return this.data;
            }
        }

        public OperationResultEventArgs(TypedData data) {
            this.data = data;
        }
    }

    public class OperationProgressEventArgs : EventArgs {
        private readonly double progress;

        public double Progress {
            get {
                return this.progress;
            }
        }

        public OperationProgressEventArgs(double progress) {
            this.progress = progress;
        }
    }
}
