using NoCap.Library.Util;

namespace NoCap.Library.Tests.TestHelpers {
    class TestTimeEstimate : ITimeEstimate {
        public double ProgressWeight {
            get;
            set;
        }

        public bool IsIndeterminant {
            get;
            set;
        }

        public TestTimeEstimate() {
        }

        public TestTimeEstimate(double progressWeight, bool isIndeterminant = false) {
            ProgressWeight = progressWeight;
            IsIndeterminant = isIndeterminant;
        }
    }
}
