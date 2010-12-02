namespace NoCap.Library.Progress {
    public static class ProgressTrackerExtensions {
        public static void BindTo(this IProgressTracker from, IMutableProgressTracker to) {
            from.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    to.Progress = from.Progress;
                }
            };
        }
    }
}