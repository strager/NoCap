using System;

namespace NoCap.Library.Progress {
    /// <summary>
    /// Provides a read-only wrapper around an <see cref="IProgressTracker"/>.
    /// </summary>
    public sealed class ReadOnlyProgressTracker : IProgressTracker {
        private readonly IProgressTracker source;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyProgressTracker"/> class.
        /// </summary>
        /// <param name="source">The progress tracker to wrap.</param>
        public ReadOnlyProgressTracker(IProgressTracker source) {
            this.source = source;

            source.ProgressUpdated += (sender, e) => OnProgressUpdated(e);
        }

        public double Progress {
            get {
                return this.source.Progress;
            }
        }

        public ITimeEstimate EstimatedTimeRemaining {
            get {
                return this.source.EstimatedTimeRemaining;
            }
        }

        public event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;

        private void OnProgressUpdated(ProgressUpdatedEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
