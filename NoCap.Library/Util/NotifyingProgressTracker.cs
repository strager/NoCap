using System;
using System.ComponentModel;

namespace NoCap.Library.Util {
    public class NotifyingProgressTracker : IMutableProgressTracker {
        private double progress;
        private readonly ITimeEstimate estimatedTimeRemaining;

        public double Progress {
            get {
                return this.progress;
            }

            set {
                this.progress = value;

                Notify("Progress");
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}