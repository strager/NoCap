using System;
using System.Collections.Generic;
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
using Bindable.Linq;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.Extensions.Default.Helpers;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Tasks;
using Windows7.DesktopIntegration;
using ICommand = NoCap.Library.ICommand;
using Separator = System.Windows.Controls.Separator;

namespace NoCap.Extensions.Default.Plugins {
    class TaskbarCommands {
        public static System.Windows.Input.ICommand ShowTasks;

        static TaskbarCommands() {
            ShowTasks = new RoutedUICommand("_Show Tasks", "ShowTasks", typeof(TaskbarCommands));
        }
    }

    [Export(typeof(IPlugin))]
    [DataContract(Name = "TaskbarPlugin")]
    sealed class TaskbarPlugin : IPlugin, IExtensibleDataObject {
        private NoCapLogo logo;
        private TaskbarIcon taskbarIcon;
        private ICommandRunner commandRunner;
        private TaskCollection taskCollection;
        private TaskPopup taskPopup;

        private bool showNotificationOnStart = true;
        private bool showNotificationOnComplete = true;

        [DataMember(Name = "ShowNotificationOnCommandStart")]
        public bool ShowNotificationOnStart {
            get { return this.showNotificationOnStart; }
            set { this.showNotificationOnStart = value; }
        }

        [DataMember(Name = "ShowNotificationOnCommandEnd")]
        public bool ShowNotificationOnComplete {
            get { return this.showNotificationOnComplete; }
            set { this.showNotificationOnComplete = value; }
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
                ),
                new System.Windows.Input.CommandBinding(
                    TaskbarCommands.ShowTasks,
                    (sender, e) => this.taskPopup.QueueShow(),
                    (sender, e) => {
                        e.CanExecute = this.taskCollection.Count != 0;
                        e.Handled = true;
                    }
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
                task.Completed += (sender, e) => OnTaskEnded();
                task.Canceled += (sender, e) => OnTaskEnded();

                if (ShowNotificationOnStart) {
                    this.taskPopup.QueueShow();
                }

                if (task.State == TaskState.Completed || task.State == TaskState.Canceled) {
                    this.taskPopup.QueueHide();
                }
            }));
        }

        private void OnTaskEnded() {
            if (ShowNotificationOnComplete) {
                this.taskPopup.QueueShow();
            }

            this.taskPopup.QueueHide();
        }

        public string Name {
            get {
                return "Taskbar";
            }
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return new TaskbarEditor {
                DataContext = this
            };
        }

        public void Initialize(IPluginContext pluginContext) {
            this.taskbarIcon = new TaskbarIcon {
                Visibility = Visibility.Visible,
                DoubleClickCommand = ApplicationCommands.Properties,
                ContextMenu = BuildContextMenu(pluginContext),
            };

            this.logo = new NoCapLogo();
            
            this.commandRunner = pluginContext.CommandRunner;

            this.commandRunner.TaskStarted     += BeginTask;
            this.commandRunner.TaskCompleted   += EndTask;
            this.commandRunner.ProgressUpdated += UpdateProgress;
            this.commandRunner.TaskCanceled    += CancelTask;

            this.taskCollection = new TaskCollection();

            this.commandRunner.TaskStarted += (sender, e) => {
                this.taskCollection.AddTask(e.Task);
            };

            this.taskPopup = new TaskPopup {
                DataContext = this.taskCollection.Select((t) => new TaskViewModel(t))
            };

            this.taskPopup.Hidden += (sender, e) => this.taskCollection.RemoveFinishedTasks();

            this.taskPopup.QueueHide();

            this.taskbarIcon.ShowCustomBalloon(this.taskPopup, PopupAnimation.None, null);

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AddBindings();

            UpdateIcon(1);
        }

        private static ContextMenu BuildContextMenu(IPluginContext pluginContext) {
            var commands = pluginContext.CommandProvider.StandAloneCommands;
            var commandMenuItems = new ObservableCollection<MenuItem>();

            BuildCommandMenuItems(commands, commandMenuItems);

            commands.CollectionChanged += (sender, e) => BuildCommandMenuItems(commands, commandMenuItems);

            return new ContextMenu {
                ItemsSource = new CompositeCollection {
                    new MenuItem { Command = TaskbarCommands.ShowTasks, Header = "_Show Running Tasks" },
                    new Separator(),
                    new CollectionContainer { Collection = commandMenuItems },
                    new Separator(),
                    new MenuItem { Command = ApplicationCommands.Properties, Header = "_Settings" },
                    new MenuItem { Command = ApplicationCommands.Close, Header = "E_xit" },
                }
            };
        }

        private static void BuildCommandMenuItems(IEnumerable<ICommand> commands, ICollection<MenuItem> commandMenuItems) {
            commandMenuItems.Clear();

            foreach (var command in commands) {
                commandMenuItems.Add(new MenuItem {
                    Command = NoCapCommands.Execute,
                    CommandParameter = command,
                    Header = command.Name,
                });
            }
        }

        public void Dispose() {
            this.taskbarIcon.Dispose();
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}
