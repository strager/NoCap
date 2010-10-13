using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NoCap.Library.Util {
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