using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NoCap.Library.Util {
    public class AggregateProgressTracker : IProgressTracker {
        private readonly IDictionary<IProgressTracker, double> progressTrackerWeights;

        private double TotalWeight {
            get {
                return this.progressTrackerWeights.Sum((kvp) => kvp.Value);
            }
        }

        public double Progress {
            get {
                double totalWeight = TotalWeight;

                if (totalWeight == 0) {
                    return ProgressTrackers.Sum((progressTracker) => progressTracker.Progress);
                }

                double progress = this.progressTrackerWeights.Sum((kvp) => kvp.Key.Progress * kvp.Value / totalWeight);

                return progress;
            }
        }

        public double EstimatedTimeRemaining {
            get {
                double currentWeight = ProgressTrackers.Sum((progressTracker) => progressTracker.EstimatedTimeRemaining);
                double totalWeight = TotalWeight;

                if (totalWeight == 0) {
                    return 0;
                }

                return currentWeight / totalWeight;
            }
        }

        public ReadOnlyCollection<IProgressTracker> ProgressTrackers {
            get {
                return new ReadOnlyCollection<IProgressTracker>(this.progressTrackerWeights.Keys.ToList());
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

            this.progressTrackerWeights = new Dictionary<IProgressTracker, double>();

            foreach (var progressTracker in progressTrackers) {
                this.progressTrackerWeights[progressTracker] = progressTracker.EstimatedTimeRemaining;
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