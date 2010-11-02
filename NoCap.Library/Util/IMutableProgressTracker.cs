namespace NoCap.Library.Util {
    public interface IMutableProgressTracker : IProgressTracker {
        new double Progress {
            get;
            set;
        }

        new double EstimatedTimeRemaining {
            get;
            set;
        }
    }
}