namespace NoCap.Library.Util {
    public interface ITimeEstimate {
        double ProgressWeight { get; }
        bool IsIndeterminate { get; }
    }

    class SimpleTimeEstimate : ITimeEstimate {
        private readonly double progressWeight;
        private readonly bool isIndeterminate;

        public double ProgressWeight {
            get {
                return this.progressWeight;
            }
        }

        public bool IsIndeterminate {
            get {
                return this.isIndeterminate;
            }
        }

        public SimpleTimeEstimate(double progressWeight, bool isIndeterminate = false) {
            this.progressWeight = progressWeight;
            this.isIndeterminate = isIndeterminate;
        }
    }

    public static class TimeEstimates {
        public static readonly ITimeEstimate Instantaneous   = new SimpleTimeEstimate(0);
        public static readonly ITimeEstimate UserInteractive = new SimpleTimeEstimate(10, true);
        public static readonly ITimeEstimate ShortOperation  = new SimpleTimeEstimate(30);
        public static readonly ITimeEstimate LongOperation   = new SimpleTimeEstimate(90);

        public static readonly ITimeEstimate Indeterminate = new SimpleTimeEstimate(0, true);
    }
}
