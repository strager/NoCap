using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NoCap.Library.Tasks;

namespace NoCap.Extensions.Default.Helpers {
    /// <summary>
    /// Interaction logic for TaskPopup.xaml
    /// </summary>
    public partial class TaskPopup {
        public static readonly TimeSpan CloseHideDelay = TimeSpan.FromMilliseconds(0);

        private readonly Storyboard showStoryboard;
        private readonly Storyboard hideStoryboard;

        private readonly object storyboardSync = new object();

        private bool isAppearing = false;

        public TaskPopup() {
            InitializeComponent();

            // HACK
            this.hideButton.Height = 0;
            Opacity = 0;

            this.showStoryboard = (Storyboard) Resources["ShowAnimation"];
            this.hideStoryboard = (Storyboard) Resources["HideAnimation"];

            this.showStoryboard.Completed += (sender, e) => { this.isAppearing = false; };
            this.hideStoryboard.Completed += (sender, e) => OnHidden(new EventArgs());
        }

        private void CancelTask(object sender, ExecutedRoutedEventArgs e) {
            var task = e.Parameter as ICommandTask;

            if (task != null) {
                task.Cancel();
            }
        }

        public void Show() {
            this.isAppearing = true;

            var dispatcher = this.showStoryboard.Dispatcher;

            if (!dispatcher.CheckAccess()) {
                dispatcher.BeginInvoke(new Action(Show));

                return;
            }

            lock (this.storyboardSync) {
                this.hideStoryboard.Stop(this);
                this.showStoryboard.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                this.hideStoryboard.Remove(this);
            }
        }

        public void Hide(TimeSpan delay) {
            var dispatcher = this.showStoryboard.Dispatcher;

            if (!dispatcher.CheckAccess()) {
                dispatcher.BeginInvoke(new Action<TimeSpan>(Hide), delay);

                return;
            }
            
            if (this.isAppearing) {
                // This mess is my effort to prevent race conditions

                EventHandler callback = null;
                int called = 0;

                callback = new EventHandler((sender, e) => {
                    if (Interlocked.CompareExchange(ref called, 1, 0) == 1) {
                        // Ensure only one call.
                        return;
                    }

                    Hide(delay);

                    this.showStoryboard.Completed -= callback;
                });

                this.showStoryboard.Completed += callback;

                if (!this.isAppearing) {
                    callback(null, null);
                }

                return;
            }

            lock (this.storyboardSync) {
                var hide = this.hideStoryboard;
                hide.BeginTime = delay;

                this.showStoryboard.Stop(this);
                hide.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
                this.showStoryboard.Remove(this);
            }
        }

        private void Hide(object sender, ExecutedRoutedEventArgs e) {
            Hide(CloseHideDelay);
        }

        public event EventHandler Hidden;

        protected void OnHidden(EventArgs e) {
            var handler = Hidden;

            if (handler != null) {
                handler(this, e);
            }
        }
    }

    class TaskViewModel : INotifyPropertyChanged {
        private readonly ICommandTask task;

        public double Progress {
            get {
                return Task.ProgressTracker.Progress;
            }
        }

        public string Status {
            get {
                return Task.ProgressTracker.Status;
            }
        }

        public string Name {
            get {
                return Task.Name;
            }
        }

        public TaskState State {
            get {
                return Task.State;
            }
        }

        public ICommandTask Task {
            get {
                return this.task;
            }
        }

        public TaskViewModel(ICommandTask task) {
            if (task == null) {
                throw new ArgumentNullException("task");
            }

            this.task = task;

            Task.ProgressTracker.ProgressUpdated += (sender, e) => Notify("Progress");
            Task.ProgressTracker.StatusUpdated   += (sender, e) => Notify("Status");

            Task.Started   += (sender, e) => Notify("State");
            Task.Completed += (sender, e) => Notify("State");
            task.Canceled  += (sender, e) => Notify("State");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
