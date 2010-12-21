using System;

namespace NoCap.Library.Progress {
    /// <summary>
    /// Tracks the progress of an operation.
    /// </summary>
    public interface IProgressTracker {
        /// <summary>
        /// Gets the progress of the operation, from 0 to 1 inclusive.
        /// </summary>
        /// <remarks>
        /// A value of 0 signifies that the operation has not yet begun.
        /// 
        /// A value of 1 signifies that the operation has completed.
        /// 
        /// Any values between 0 and 1 signify that the operation is progressing.
        /// </remarks>
        /// <value>The progress of the operation.</value>
        double Progress { get; }

        string Status { get; }

        event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;
        event EventHandler<StatusUpdatedEventArgs> StatusUpdated;
    }

    public class StatusUpdatedEventArgs : EventArgs {
        private readonly string status;

        public StatusUpdatedEventArgs(string status) {
            this.status = status;
        }

        public string Status {
            get {
                return this.status;
            }
        }
    }

    public class ProgressUpdatedEventArgs : EventArgs {
        private readonly double progress;

        public ProgressUpdatedEventArgs(double progress) {
            this.progress = progress;
        }

        public double Progress {
            get {
                return this.progress;
            }
        }
    }
}