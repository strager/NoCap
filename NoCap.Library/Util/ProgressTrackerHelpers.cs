namespace NoCap.Library.Util {
    public static class ProgressTrackerHelpers {
        public static void BindTo(this IProgressTracker from, IMutableProgressTracker to) {
            from.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    to.Progress = from.Progress;
                }
            };
        }
    }
}