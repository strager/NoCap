﻿using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NoCap.Library;
using NoCap.Library.Tasks;

namespace NoCap.Extensions.Default.Helpers {
    /// <summary>
    /// Interaction logic for TaskPopup.xaml
    /// </summary>
    public partial class TaskPopup {
        private readonly StoryboardQueue storyboardQueue = new StoryboardQueue();
        private readonly Storyboard showStoryboard;
        private readonly Storyboard hideStoryboard;

        public TaskPopup() {
            InitializeComponent();

            // HACK
            this.hideButton.Height = 0;
            Opacity = 0;

            CommandBindings.Add(new CommandBinding(
                NoCapCommands.Cancel,
                (sender, e) => {
                    var task = ((TaskbarPopupViewModel) DataContext).Task;

                    task.Cancel();
                }
            ));

            CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Close,
                (sender, e) => QueueHide()
            ));

            this.showStoryboard = (Storyboard) Resources["ShowAnimation"];
            this.hideStoryboard = (Storyboard) Resources["HideAnimation"];
        }

        public void QueueShow() {
            this.storyboardQueue.Enqueue(this.showStoryboard, this);
        }

        public void QueueHide() {
            this.storyboardQueue.Enqueue(this.hideStoryboard, this, false);
        }

        private void QueueHide(object sender, ExecutedRoutedEventArgs e) {
            QueueHide();
        }
    }

    class TaskbarPopupViewModel : INotifyPropertyChanged {
        private readonly ICommandTask task;

        public double Progress {
            get {
                return Task.ProgressTracker.Progress;
            }
        }

        public ICommandTask Task {
            get {
                return this.task;
            }
        }

        public TaskbarPopupViewModel(ICommandTask task) {
            if (task == null) {
                throw new ArgumentNullException("task");
            }

            this.task = task;

            Task.ProgressTracker.ProgressUpdated += (sender, e) => Notify("Progress");
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
