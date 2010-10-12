using System;

namespace NoCap.Library {
    public interface IOperation<T> {
        double Progress { get; }
        OperationState State { get; }

        Exception Exception { get; }

        T Data { get; }

        event EventHandler<OperationProgressEventArgs> ProgressUpdated;
        event EventHandler<OperationResultEventArgs<T>> Completed;

        void Start();
        void Cancel();
    }

    public enum OperationState {
        NotYetStarted = 0,  // TODO Better name
        Started,
        Completed,
        Canceled
    }

    public class OperationResultEventArgs<T> : EventArgs {
        private readonly T data;

        public T Data {
            get {
                return this.data;
            }
        }

        public OperationResultEventArgs(T data) {
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
