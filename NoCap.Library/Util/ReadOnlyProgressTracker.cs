using System.ComponentModel;

namespace NoCap.Library.Util {
    public sealed class ReadOnlyProgressTracker : IProgressTracker {
        private readonly IProgressTracker source;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyProgressTracker(IProgressTracker source) {
            this.source = source;

            source.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress" || e.PropertyName == "EstimatedTimeRemaining") {
                    Notify(e.PropertyName);
                }
            };
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

        private void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
