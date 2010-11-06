using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NoCap.Library.Util {
    class AggregateProgressTrackerTimeEstimate : ITimeEstimate {
        private readonly AggregateProgressTracker aggregateProgressTracker;

        public AggregateProgressTrackerTimeEstimate(AggregateProgressTracker aggregateProgressTracker) {
            this.aggregateProgressTracker = aggregateProgressTracker;
        }

        public double ProgressWeight {
            get {
                return this.aggregateProgressTracker.ProgressTrackers.Sum((progressTracker) => progressTracker.EstimatedTimeRemaining.ProgressWeight);
            }
        }

        public bool IsIndeterminate {
            get {
                throw new NotImplementedException();
            }
        }
    }

    public class AggregateProgressTracker : IProgressTracker {
        private readonly ITimeEstimate timeEstimate;
        private readonly IList<IProgressTracker> progressTrackers;

        public double Progress {
            get {
                return this.progressTrackers.Sum(
                    (progressTracker) => progressTracker.Progress * progressTracker.EstimatedTimeRemaining.ProgressWeight
                ) / EstimatedTimeRemaining.ProgressWeight;
            }
        }

        public ITimeEstimate EstimatedTimeRemaining {
            get {
                return this.timeEstimate;
            }
        }

        public ReadOnlyCollection<IProgressTracker> ProgressTrackers {
            get {
                return new ReadOnlyCollection<IProgressTracker>(this.progressTrackers);
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

            this.timeEstimate = new AggregateProgressTrackerTimeEstimate(this);
            this.progressTrackers = progressTrackers.ToArray();

            foreach (var progressTracker in progressTrackers) {
                progressTracker.PropertyChanged += TrackedProgressChanged;
            }
        }

        private void TrackedProgressChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Progress":
                    Notify("Progress");

                    break;

                case "EstimatedTimeRemaining":
                    Notify("Progress");
                    Notify("EstimatedTimeRemaining");

                    break;
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