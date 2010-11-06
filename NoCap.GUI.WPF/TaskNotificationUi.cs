using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.Library;
using Windows7.DesktopIntegration;

namespace NoCap.GUI.WPF {
    class TaskNotificationUi : IDisposable {
        private readonly NoCapLogo logo;
        private readonly TaskbarIcon taskbarIcon;

        private readonly ICollection<CommandRunner> boundCommandRunners = new List<CommandRunner>();

        private bool isDisposed;

        public TaskNotificationUi(TaskbarIcon taskbarIcon, NoCapLogo logo) {
            this.taskbarIcon = taskbarIcon;
            this.logo = logo;

            UpdateIcon(1);
        }

        public void BindFrom(CommandRunner commandRunner) {
            BindFrom(commandRunner, true);
        }

        private void BindFrom(CommandRunner commandRunner, bool track) {
            EnsureNotDisposed();

            commandRunner.TaskStarted     += BeginTask;
            commandRunner.TaskCompleted   += EndTask;
            commandRunner.ProgressUpdated += UpdateProgress;

            if (track) {
                this.boundCommandRunners.Add(commandRunner);
            }
        }

        public void UnbindFrom(CommandRunner commandRunner) {
            UnbindFrom(commandRunner, true);
        }

        private void UnbindFrom(CommandRunner commandRunner, bool untrack) {
            EnsureNotDisposed();

            commandRunner.TaskStarted     -= BeginTask;
            commandRunner.TaskCompleted   -= EndTask;
            commandRunner.ProgressUpdated -= UpdateProgress;

            if (untrack) {
                this.boundCommandRunners.Remove(commandRunner);
            }
        }

        public void BeginTask(object sender, CommandTaskEventArgs e) {
            if (this.isDisposed) {
                return;
            }

            ShowTaskPopup(e.Task);
        }

        public void EndTask(object sender, CommandTaskEventArgs e) {
            if (this.isDisposed) {
                return;
            }

            // Do nothing
        }

        public void UpdateProgress(object sender, CommandTaskProgressEventArgs e) {
            if (this.isDisposed) {
                return;
            }

            double progress = e.Progress;

            UpdateWindows(progress);
            UpdateIcon(progress);
            UpdateIconToolTip(progress);
        }

        private void UpdateWindows(double progress) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                foreach (var window in Application.Current.Windows) {
                    SetWindowProgress((Window) window, progress);
                }
            }));
        }

        private void UpdateIcon(double progress) {
            this.logo.Dispatcher.BeginInvoke(new Action(() => {
                this.logo.Progress = progress;

                this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                    this.taskbarIcon.IconSource = this.logo.MakeIcon(128);
                }));
            }));
        }

        private void UpdateIconToolTip(double progress) {
            this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                this.taskbarIcon.ToolTipText = string.Format("Progress: {0}%", progress * 100);
            }));
        }

        private static void SetWindowProgress(Window window, double progress) {
            window.Dispatcher.BeginInvoke(new Action(() => {
                var handle = new WindowInteropHelper(window).Handle;

                if (progress >= 1) {
                    Windows7Taskbar.SetProgressState(handle, Windows7Taskbar.ThumbnailProgressState.NoProgress);
                } else {
                    Windows7Taskbar.SetProgressState(handle, Windows7Taskbar.ThumbnailProgressState.Normal);

                    const ulong max = 9001;

                    Windows7Taskbar.SetProgressValue(handle, (ulong) (progress * max), max);
                }
            }));
        }

        private void ShowTaskPopup(ICommandTask task) {
            this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                var taskPopup = new TaskPopup {
                    DataContext = task
                };

                task.Completed += (sender, e) => taskPopup.QueueClose();
                task.Cancelled += (sender, e) => taskPopup.QueueClose();

                if (task.State == TaskState.Completed || task.State == TaskState.Cancelled) {
                    taskPopup.QueueClose();
                }

                this.taskbarIcon.ShowCustomBalloon(taskPopup, PopupAnimation.Fade, null);
            }));
        }

        public void Dispose() {
            foreach (var commandRunner in this.boundCommandRunners) {
                UnbindFrom(commandRunner, false);
            }

            this.boundCommandRunners.Clear();
            this.isDisposed = true;
        }

        private void EnsureNotDisposed() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("TaskNotificationUi");
            }
        }
    }
}
