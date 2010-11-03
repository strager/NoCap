namespace NoCap.Library.Util {
    public interface ITimeEstimate {
        double ProgressWeight { get; }
        bool IsIndeterminant { get; }
    }

    class SimpleTimeEstimate : ITimeEstimate {
        private readonly double progressWeight;
        private readonly bool isIndeterminant;

        public double ProgressWeight {
            get {
                return this.progressWeight;
            }
        }

        public bool IsIndeterminant {
            get {
                return this.isIndeterminant;
            }
        }

        public SimpleTimeEstimate(double progressWeight, bool isIndeterminant = false) {
            this.progressWeight = progressWeight;
            this.isIndeterminant = isIndeterminant;
        }
    }

    public static class TimeEstimates {
        public static ITimeEstimate Instantanious   = new SimpleTimeEstimate(0);
        public static ITimeEstimate UserInteractive = new SimpleTimeEstimate(10, true);
        public static ITimeEstimate ShortOperation  = new SimpleTimeEstimate(30);
        public static ITimeEstimate LongOperation   = new SimpleTimeEstimate(90);

        public static ITimeEstimate Indeterminant = new SimpleTimeEstimate(0, true);
    }
}
