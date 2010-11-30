using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.Extensions.Default.Helpers;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Tasks;
using Windows7.DesktopIntegration;
using ICommand = NoCap.Library.ICommand;
using Separator = System.Windows.Controls.Separator;

namespace NoCap.Extensions.Default.Plugins {
    [Export(typeof(IPlugin)), Serializable]
    class TaskbarPlugin : IPlugin, ISerializable {
        [NonSerialized]
        private NoCapLogo logo;

        [NonSerialized]
        private TaskbarIcon taskbarIcon;

        [NonSerialized]
        private ObservableCollection<MenuItem> commandMenuItems;

        [NonSerialized]
        private CommandRunner commandRunner;

        public TaskbarPlugin() {
        }

        public TaskbarPlugin(SerializationInfo info, StreamingContext context) {
            // Do nothing.
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            // Do nothing.
        }

        private void AddBindings() {
            var app = Application.Current;

            this.taskbarIcon.CommandBindings.AddRange(new [] {
                new System.Windows.Input.CommandBinding(
                    ApplicationCommands.Close,
                    (sender, e) => app.Shutdown(0)
                ),
                new System.Windows.Input.CommandBinding(
                    ApplicationCommands.Properties,
                    (sender, e) => app.GetType().InvokeMember(
                        "ShowSettings",             // Name
                        BindingFlags.InvokeMethod,  // Binding flags
                        null,                       // Binder
                        app,                        // This
                        new object[] { }            // Arguments
                    )
                ),
                new System.Windows.Input.CommandBinding(
                    NoCapCommands.Execute,
                    (sender, e) => this.commandRunner.Run((ICommand) e.Parameter)
                )
            });
        }

        public void BeginTask(object sender, CommandTaskEventArgs e) {
            ShowTaskPopup(e.Task);
        }

        public void EndTask(object sender, CommandTaskEventArgs e) {
            // Do nothing
        }

        private void CancelTask(object sender, CommandTaskCancellationEventArgs e) {
            UpdateProgress(1);
        }

        public void UpdateProgress(object sender, CommandTaskProgressEventArgs e) {
            double progress = e.Progress;

            UpdateProgress(progress);
        }

        private void UpdateProgress(double progress) {
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

                task.Completed += (sender, e) => {
                    taskPopup.QueueShow();
                    taskPopup.QueueHide();
                };

                task.Canceled += (sender, e) => taskPopup.QueueHide();

                this.taskbarIcon.ShowCustomBalloon(taskPopup, PopupAnimation.None, null);

                taskPopup.QueueShow();

                if (task.State == TaskState.Completed || task.State == TaskState.Canceled) {
                    taskPopup.QueueHide();
                }
            }));
        }

        public string Name {
            get {
                return "Taskbar";
            }
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return null;
        }

        public void Initialize(IPluginContext pluginContext) {
            this.commandMenuItems = new ObservableCollection<MenuItem>();

            // TODO Observe StandAloneCommands
            foreach (var command in pluginContext.CommandProvider.StandAloneCommands) {
                this.commandMenuItems.Add(new MenuItem {
                    Command = NoCapCommands.Execute,
                    CommandParameter = command,
                    Header = command.Name,
                });
            }

            var taskbarMenu = new ContextMenu {
                ItemsSource = new CompositeCollection {
                    new CollectionContainer { Collection = this.commandMenuItems },
                    new Separator(),
                    new MenuItem { Command = ApplicationCommands.Close, Header = "E_xit" },
                }
            };

            this.taskbarIcon = new TaskbarIcon {
                Visibility = Visibility.Visible,
                DoubleClickCommand = ApplicationCommands.Properties,
                ContextMenu = taskbarMenu
            };

            this.logo = new NoCapLogo();
            
            this.commandRunner = pluginContext.CommandRunner;

            this.commandRunner.TaskStarted     += BeginTask;
            this.commandRunner.TaskCompleted   += EndTask;
            this.commandRunner.ProgressUpdated += UpdateProgress;
            this.commandRunner.TaskCanceled    += CancelTask;

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AddBindings();

            UpdateIcon(1);
        }

        public void Dispose() {
            this.taskbarIcon.Dispose();
        }
    }
}
