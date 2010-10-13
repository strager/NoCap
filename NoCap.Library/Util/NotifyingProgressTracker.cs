using System.ComponentModel;

namespace NoCap.Library.Util {
    public class NotifyingProgressTracker : IMutableProgressTracker {
        private double progress;

        public double Progress {
            get {
                return this.progress;
            }

            set {
                this.progress = value;

                Notify("Progress");
            }
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