using System.ComponentModel;

namespace NoCap.Library.Util {
    public interface IProgressTracker : INotifyPropertyChanged {
        double Progress {
            get;
        }

        ITimeEstimate EstimatedTimeRemaining {
            get;
        }
    }
}