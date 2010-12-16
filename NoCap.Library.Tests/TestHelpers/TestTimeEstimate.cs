using NoCap.Library.Progress;

namespace NoCap.Library.Tests.TestHelpers {
    class TestTimeEstimate : ITimeEstimate {
        public double ProgressWeight {
            get;
            set;
        }

        public bool IsIndeterminate {
            get;
            set;
        }

        public TestTimeEstimate() {
        }

        public TestTimeEstimate(double progressWeight, bool isIndeterminate = false) {
            ProgressWeight = progressWeight;
            IsIndeterminate = isIndeterminate;
        }
    }
}
