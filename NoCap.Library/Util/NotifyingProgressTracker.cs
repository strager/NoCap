using System.ComponentModel;

namespace NoCap.Library.Util {
    public class NotifyingProgressTracker : IMutableProgressTracker {
        private double progress;
        private double estimatedTimeRemaining;

        public double Progress {
            get {
                return this.progress;
            }

            set {
                this.progress = value;

                Notify("Progress");
            }
        }

        public double EstimatedTimeRemaining {
            get {
                return this.estimatedTimeRemaining;
            }

            set {
                this.estimatedTimeRemaining = value;

                Notify("EstimatedTimeRemaining");
            }
        }

        public NotifyingProgressTracker() :
            this(TimeEstimate.NoTimeAtAll) {
        }

        public NotifyingProgressTracker(TimeEstimate timeEstimate) {
            this.estimatedTimeRemaining = (int) timeEstimate;
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