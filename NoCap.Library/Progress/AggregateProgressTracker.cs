using System;
using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library.Progress {
    // TODO Better name
    // TODO Something nicer for users?
    public struct AggregateProgressTrackerInformation {
        private readonly IProgressTracker progressTracker;
        private readonly double weight;

        public AggregateProgressTrackerInformation(IProgressTracker progressTracker, double weight) {
            this.progressTracker = progressTracker;
            this.weight = weight;
        }

        public IProgressTracker ProgressTracker {
            get {
                return this.progressTracker;
            }
        }

        public double Weight {
            get {
                return this.weight;
            }
        }
    }

    /// <summary>
    /// Aggregates multiple progress trackers into one.
    /// </summary>
    public class AggregateProgressTracker : IProgressTracker {
        private readonly IEnumerable<AggregateProgressTrackerInformation> progressTrackers;

        /// <summary>
        /// Gets the total progress all sub-operations, from 0 to 1.
        /// </summary>
        /// <value>The progress of the operation.</value>
        /// <remarks>
        /// Operations are weighed by the values given in the constructor.
        /// </remarks>
        public double Progress {
            get {
                var totalWeight = this.progressTrackers.Sum((kvp) => kvp.Weight);

                return this.progressTrackers.Sum((kvp) => kvp.ProgressTracker.Progress * kvp.Weight) / totalWeight;
            }
        }

        /// <summary>
        /// Gets the progress trackers which this instance aggregates.
        /// </summary>
        /// <value>The progress trackers.</value>
        public IEnumerable<IProgressTracker> ProgressTrackers {
            get {
                return this.progressTrackers.Select((kvp) => kvp.ProgressTracker).ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateProgressTracker"/> class.
        /// </summary>
        /// <param name="progressTrackers">The progress trackers to aggregate with their weights.</param>
        public AggregateProgressTracker(IEnumerable<AggregateProgressTrackerInformation> progressTrackers) {
            if (progressTrackers == null) {
                throw new ArgumentNullException("progressTrackers");
            }

            this.progressTrackers = progressTrackers.ToArray();

            foreach (var kvp in this.progressTrackers) {
                kvp.ProgressTracker.ProgressUpdated +=
                    (sender, e) => OnProgressUpdated(new ProgressUpdatedEventArgs(Progress));
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