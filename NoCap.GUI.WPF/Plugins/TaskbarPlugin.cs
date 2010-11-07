using System;
using System.ComponentModel.Composition.Hosting;
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
    class TaskbarPlugin : IPlugin {
        private readonly NoCapLogo logo;
        private readonly TaskbarIcon taskbarIcon;

        public TaskbarPlugin(App app) {
            var taskbarMenu = new ContextMenu();
            taskbarMenu.Items.Add(new MenuItem { Command = ApplicationCommands.Close, Header = "E_xit" });

            this.taskbarIcon = new TaskbarIcon {
                Visibility = Visibility.Visible,
                DoubleClickCommand = ApplicationCommands.Properties,
                ContextMenu = taskbarMenu
            };

            this.logo = new NoCapLogo();
            
            AddBindings(app);

            UpdateIcon(1);
        }

        private void AddBindings(App app) {
            this.taskbarIcon.CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Close,
                (sender, e) => app.Shutdown(0)
            ));
            
            this.taskbarIcon.CommandBindings.Add(new System.Windows.Input.CommandBinding(ApplicationCommands.Properties,
                (sender, e) => app.ShowSettingsEditor()
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

        private CommandRunner commandRunner;

        public CommandRunner CommandRunner {
            get {
                return this.commandRunner;
            }

            set {
                if (this.commandRunner != null) {
                    this.commandRunner.TaskStarted     -= BeginTask;
                    this.commandRunner.TaskCompleted   -= EndTask;
                    this.commandRunner.ProgressUpdated -= UpdateProgress;
                }

                this.commandRunner = value;

                if (this.commandRunner != null) {
                    this.commandRunner.TaskStarted     += BeginTask;
                    this.commandRunner.TaskCompleted   += EndTask;
                    this.commandRunner.ProgressUpdated += UpdateProgress;
                }
            }
        }

        public void Populate(CompositionContainer compositionContainer) {
            // Do nothing.
        }

        public UIElement GetEditor(IInfoStuff infoStuff) {
            return null;
        }

        public void SetUp() {
        }

        public void ShutDown() {
        }

        public void Dispose() {
            this.taskbarIcon.Dispose();
        }
    }
}
