using System;

namespace NoCap.Library.Progress {
    public sealed class NotifyingProgressTracker : IMutableProgressTracker {
        private double progress;
        private readonly ITimeEstimate estimatedTimeRemaining;

        public double Progress {
            get {
                return this.progress;
            }

            set {
                this.progress = value;

                OnProgressUpdated(new ProgressUpdatedEventArgs(value));
            }
        }

        public ITimeEstimate EstimatedTimeRemaining {
            get {
                return this.estimatedTimeRemaining;
            }
        }

        public NotifyingProgressTracker() :
            this(TimeEstimates.Indeterminate) {
        }

        public NotifyingProgressTracker(ITimeEstimate timeEstimate) {
            if (timeEstimate == null) {
                throw new ArgumentNullException("timeEstimate");
            }

            this.estimatedTimeRemaining = timeEstimate;
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