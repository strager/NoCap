using System;

namespace NoCap.Library.Progress {
    public sealed class MutableProgressTracker : IMutableProgressTracker {
        private double progress;

        public double Progress {
            get {
                return this.progress;
            }

            set {
                this.progress = value;

                OnProgressUpdated(new ProgressUpdatedEventArgs(value));
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