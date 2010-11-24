using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.Library;
using NoCap.Library.Tasks;
using Windows7.DesktopIntegration;

namespace NoCap.GUI.WPF.Plugins {
    [Export(typeof(IPlugin)), Serializable]
    class TaskbarPlugin : IPlugin, ISerializable {
        [NonSerialized]
        private NoCapLogo logo;

        [NonSerialized]
        private TaskbarIcon taskbarIcon;

        public TaskbarPlugin() {
        }

        public TaskbarPlugin(SerializationInfo info, StreamingContext context) {
            // Do nothing.
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            // Do nothing.
        }

        private void AddBindings() {
            var app = (App) Application.Current;

            this.taskbarIcon.CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Close,
                (sender, e) => app.Shutdown(0)
            ));
            
            this.taskbarIcon.CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Properties,
                (sender, e) => app.ShowSettings()
            ));
        }

        public void BeginTask(object sender, CommandTaskEventArgs e) {
            ShowTaskPopup(e.Task);
        }

        public void EndTask(object sender, CommandTaskEventArgs e) {
            // Do nothing
        }

        public void UpdateProgress(object sender, CommandTaskProgressEventArgs e) {
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
                task.Canceled += (sender, e) => taskPopup.QueueClose();

                if (task.State == TaskState.Completed || task.State == TaskState.Canceled) {
                    taskPopup.QueueClose();
                }

                this.taskbarIcon.ShowCustomBalloon(taskPopup, PopupAnimation.Fade, null);
            }));
        }

        public string Name {
            get {
                return "Taskbar";
            }
        }

        public UIElement GetEditor(IInfoStuff infoStuff) {
            return null;
        }

        public void Initialize(IRuntimePluginInfo runtimePluginInfo) {
            var taskbarMenu = new ContextMenu();
            taskbarMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Close, Header = "E_xit" });

            this.taskbarIcon = new TaskbarIcon {
                Visibility = Visibility.Visible,
                DoubleClickCommand = ApplicationCommands.Properties,
                ContextMenu = taskbarMenu
            };

            this.logo = new NoCapLogo();
            
            var commandRunner = runtimePluginInfo.CommandRunner;

            commandRunner.TaskStarted     += BeginTask;
            commandRunner.TaskCompleted   += EndTask;
            commandRunner.ProgressUpdated += UpdateProgress;

            AddBindings();

            UpdateIcon(1);
        }

        public void Dispose() {
            this.taskbarIcon.Dispose();
        }
    }
}
