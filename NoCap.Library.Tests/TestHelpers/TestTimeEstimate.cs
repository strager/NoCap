using NoCap.Library.Progress;
using NoCap.Library.Util;

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
            this.IsIndeterminate = isIndeterminate;
        }
    }
}
