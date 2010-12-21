using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NoCap.Library.Progress {
    public struct ProgressTrackerCollectionItem {
        private readonly IProgressTracker progressTracker;
        private readonly double weight;

        public ProgressTrackerCollectionItem(IProgressTracker progressTracker, double weight) {
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

    public class ProgressTrackerCollection : ICollection<ProgressTrackerCollectionItem> {
        private readonly ICollection<ProgressTrackerCollectionItem> items = new List<ProgressTrackerCollectionItem>();

        public IEnumerator<ProgressTrackerCollectionItem> GetEnumerator() {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(ProgressTrackerCollectionItem item) {
            this.items.Add(item);
        }

        public void Add(IProgressTracker progressTracker, double weight) {
            Add(new ProgressTrackerCollectionItem(progressTracker, weight));
        }

        public void Clear() {
            this.items.Clear();
        }

        bool ICollection<ProgressTrackerCollectionItem>.Contains(ProgressTrackerCollectionItem item) {
            return this.items.Contains(item);
        }

        public bool Contains(IProgressTracker progressTracker) {
            return this.items.Any((item) => item.ProgressTracker == progressTracker);
        }

        void ICollection<ProgressTrackerCollectionItem>.CopyTo(ProgressTrackerCollectionItem[] array, int arrayIndex) {
            this.items.CopyTo(array, arrayIndex);
        }

        bool ICollection<ProgressTrackerCollectionItem>.Remove(ProgressTrackerCollectionItem item) {
            return this.items.Remove(item);
        }

        public int Count {
            get {
                return this.items.Count;
            }
        }

        bool ICollection<ProgressTrackerCollectionItem>.IsReadOnly {
            get {
                return false;
            }
        }
    }

    /// <summary>
    /// Aggregates multiple progress trackers into one.
    /// </summary>
    public class AggregateProgressTracker : IProgressTracker {
        private readonly IEnumerable<ProgressTrackerCollectionItem> progressTrackers;

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

        public string Status {
            get;
            private set;
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
        public AggregateProgressTracker(IEnumerable<ProgressTrackerCollectionItem> progressTrackers) {
            if (progressTrackers == null) {
                throw new ArgumentNullException("progressTrackers");
            }

            this.progressTrackers = progressTrackers.ToArray(); // Make a copy just in case

            foreach (var kvp in this.progressTrackers) {
                kvp.ProgressTracker.ProgressUpdated += ChildProgressUpdated;
                kvp.ProgressTracker.StatusUpdated   += ChildStatusUpdated;
            }
        }

        private void ChildProgressUpdated(object sender, EventArgs e) {
            OnProgressUpdated(new ProgressUpdatedEventArgs(Progress));
        }

        private void ChildStatusUpdated(object sender, StatusUpdatedEventArgs e) {
            Status = e.Status;

            OnStatusUpdated(new StatusUpdatedEventArgs(Status));
        }

        public event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;
        public event EventHandler<StatusUpdatedEventArgs> StatusUpdated;

        private void OnProgressUpdated(ProgressUpdatedEventArgs e) {
            var handler = ProgressUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }

        private void OnStatusUpdated(StatusUpdatedEventArgs e) {
            var handler = StatusUpdated;

            if (handler != null) {
                handler(this, e);
            }
        }
    }
}