namespace NoCap.Library.Progress {
    public static class ProgressTrackerExtensions {
        public static void BindTo(this IProgressTracker from, IMutableProgressTracker to) {
            from.ProgressUpdated += (sender, e) => {
                to.Progress = e.Progress;
            };
        }
    }
}