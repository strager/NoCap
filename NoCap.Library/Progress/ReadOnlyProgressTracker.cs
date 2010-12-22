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
            source.StatusUpdated   += (sender, e) => OnStatusUpdated(e);
        }

        public double Progress {
            get {
                return this.source.Progress;
            }
        }

        public string Status {
            get {
                return this.source.Status;
            }
        }

        public event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;
        public event EventHandler<StatusUpdatedEventArgs> StatusUpdated;

        private void OnProgressUpdated(ProgressUpdatedEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }

        private void OnStatusUpdated(StatusUpdatedEventArgs e) {
            var handler = StatusUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
