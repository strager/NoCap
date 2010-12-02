using System.ComponentModel;

namespace NoCap.Library.Progress {
    /// <summary>
    /// Tracks the progress of an operation.
    /// </summary>
    public interface IProgressTracker : INotifyPropertyChanged {
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

        // FIXME Why is this here?
        /// <summary>
        /// Gets the estimated time remaining for the operation.
        /// </summary>
        /// <value>The estimated time remaining.</value>
        ITimeEstimate EstimatedTimeRemaining { get; }
    }
}