using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace NoCap.Library {
    public interface IProgressTracker {
        double Progress {
            get;
        }
    }

    class SimpleProgressTracker : IProgressTracker {
        public double Progress {
            get;
            set;
        }
    }

    class AggregateProgressTracker : IProgressTracker {
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

        public AggregateProgressTracker(IEnumerable<IProgressTracker> progressTrackers) {
            this.progressTrackers = new ReadOnlyCollection<IProgressTracker>(progressTrackers.ToList());
        }
    }
}
