namespace NoCap.Library.Progress {
    /// <summary>
    /// Allows notifying of the progress of an operation.
    /// </summary>
    public interface IMutableProgressTracker : IProgressTracker {
        /// <summary>
        /// Gets or sets the progress of the operation, from 0 to 1 inclusive.
        /// </summary>
        /// <remarks>
        /// A value of 0 signifies that the operation has not yet begun.
        /// 
        /// A value of 1 signifies that the operation has completed.
        /// 
        /// Any values between 0 and 1 signify that the operation is progressing.
        /// </remarks>
        /// <value>The progress of the operation.</value>
        new double Progress { get; set; }

        new string Status { get; set; }
    }
}