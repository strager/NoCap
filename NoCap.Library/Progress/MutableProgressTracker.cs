using System;

namespace NoCap.Library.Progress {
    public sealed class MutableProgressTracker : IMutableProgressTracker {
        private double progress;
        private string status;

        public double Progress {
            get {
                return this.progress;
            }

            set {
                this.progress = value;

                OnProgressUpdated(new ProgressUpdatedEventArgs(value));
            }
        }

        public string Status {
            get {
                return this.status;
            }

            set {
                this.status = value;

                OnStatusUpdated(new StatusUpdatedEventArgs(value));
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