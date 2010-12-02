using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NoCap.Library.Progress {
    [Obsolete]
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

    /// <summary>
    /// Aggregates multiple progress trackers into one.
    /// </summary>
    public class AggregateProgressTracker : IProgressTracker {
        private readonly ITimeEstimate timeEstimate;
        private readonly IList<IProgressTracker> progressTrackers;

        /// <summary>
        /// Gets the total progress all sub-operations, from 0 to 1.
        /// </summary>
        /// <value>The progress of the operation.</value>
        /// <remarks>
        /// Operations are weighed by <see cref="IProgressTracker.EstimatedTimeRemaining.ProgressWeight"/>.
        /// </remarks>
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

        /// <summary>
        /// Gets the progress trackers which this instance aggregates.
        /// </summary>
        /// <value>The progress trackers.</value>
        public ReadOnlyCollection<IProgressTracker> ProgressTrackers {
            get {
                return new ReadOnlyCollection<IProgressTracker>(this.progressTrackers);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateProgressTracker"/> class.
        /// </summary>
        /// <param name="progressTrackers">The progress trackers to aggregate.</param>
        public AggregateProgressTracker(params IProgressTracker[] progressTrackers) :
            this((IEnumerable<IProgressTracker>) progressTrackers) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateProgressTracker"/> class.
        /// </summary>
        /// <param name="progressTrackers">The progress trackers to aggregate.</param>
        public AggregateProgressTracker(IEnumerable<IProgressTracker> progressTrackers) {
            if (progressTrackers == null) {
                throw new ArgumentNullException("progressTrackers");
            }

            this.timeEstimate = new AggregateProgressTrackerTimeEstimate(this);
            this.progressTrackers = progressTrackers.ToArray();

            foreach (var progressTracker in progressTrackers) {
                progressTracker.ProgressUpdated +=
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