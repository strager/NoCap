using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace NoCap.Library {
    public interface IProgressTracker : INotifyPropertyChanged {
        double Progress {
            get;
        }
    }

    public interface IMutableProgressTracker : IProgressTracker {
        new double Progress {
            get;
            set;
        }
    }

    public class NullProgressTracker : IMutableProgressTracker {
        public event PropertyChangedEventHandler PropertyChanged;

        public double Progress {
            get {
                return 0;
            }

            set {
                // Do nothing
            }
        }
    }

    public static class ProgressTrackerHelpers {
        public static void BindTo(this IProgressTracker from, IMutableProgressTracker to) {
            from.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    to.Progress = from.Progress;
                }
            };
        }
    }

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

    public class AggregateProgressTracker : IProgressTracker {
        private readonly ReadOnlyCollection<IProgressTracker> progressTrackers;

        public double Progress {
            get {
                return ProgressTrackers.Average((progressTracker) => progressTracker.Progress);
            }
        }

        public ReadOnlyCollection<IProgressTracker> ProgressTrackers {
            get {
                return this.progressTrackers;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AggregateProgressTracker(params IProgressTracker[] progressTrackers) :
            this((IEnumerable<IProgressTracker>) progressTrackers) {
        }

        public AggregateProgressTracker(IEnumerable<IProgressTracker> progressTrackers) {
            if (progressTrackers == null) {
                throw new ArgumentNullException("progressTrackers");
            }

            this.progressTrackers = new ReadOnlyCollection<IProgressTracker>(progressTrackers.ToList());

            foreach (var notifyingProgressTracker in ProgressTrackers) {
                notifyingProgressTracker.PropertyChanged += TrackedProgressChanged;
            }
        }

        private void TrackedProgressChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Progress") {
                Notify("Progress");
            }
        }

        public void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
