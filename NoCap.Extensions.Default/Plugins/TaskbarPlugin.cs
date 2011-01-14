using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using Bindable.Linq;
using Hardcodet.Wpf.TaskbarNotification;
using log4net;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(TaskbarPlugin));

        public static readonly TimeSpan CompleteHideDelay = TimeSpan.FromSeconds(2);
        public static readonly TimeSpan CancelHideDelay = TimeSpan.FromSeconds(5);

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

        public void BeginTask(object sender, CommandTaskEventArgs e) {
            ShowTaskPopup(e.Task);
        }

        public void EndTask(object sender, CommandTaskEventArgs e) {
            // Do nothing
        }

        private void CancelTask(object sender, CommandTaskCancellationEventArgs e) {
            UpdateProgress(1);
        }

        public void UpdateProgress(object sender, EventArgs e) {
            var progress = this.taskCollection.Progress;

            if (this.taskCollection.Count == 0) {
                // If there are no tasks, show we are done with all tasks.
                progress = 1;
            }

            UpdateProgress(progress);
        }

        private void UpdateProgress(double progress) {
            UpdateWindows(progress);
            UpdateIcon(progress);
            UpdateIconToolTip(progress);
        }

        private void UpdateWindows(double progress) {
            var dispatcher = Application.Current.Dispatcher;

            if (!dispatcher.CheckAccess()) {
                dispatcher.BeginInvoke(new Action<double>(UpdateWindows), progress);

                return;
            }

            foreach (var window in Application.Current.Windows) {
                SetWindowProgress((Window) window, progress);
            }
        }

        private void UpdateIcon(double progress) {
            var dispatcher = this.logo.Dispatcher;

            if (!dispatcher.CheckAccess()) {
                dispatcher.BeginInvoke(new Action<double>(UpdateIcon), progress);

                return;
            }

            this.logo.Progress = progress;

            this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                this.taskbarIcon.IconSource = this.logo.MakeIcon(128);
            }));
        }

        private void UpdateIconToolTip(double progress) {
            this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                this.taskbarIcon.ToolTipText = string.Format("Progress: {0}%", progress * 100);
            }));
        }

        private static void SetWindowProgress(Window window, double progress) {
            var dispatcher = window.Dispatcher;

            if (!dispatcher.CheckAccess()) {
                dispatcher.BeginInvoke(new Action<Window, double>(SetWindowProgress), window, progress);

                return;
            }

            try {
                var handle = new WindowInteropHelper(window).Handle;

                if (progress >= 1) {
                    Windows7Taskbar.SetProgressState(handle, Windows7Taskbar.ThumbnailProgressState.NoProgress);
                } else {
                    Windows7Taskbar.SetProgressState(handle, Windows7Taskbar.ThumbnailProgressState.Normal);

                    const ulong max = 9001;

                    Windows7Taskbar.SetProgressValue(handle, (ulong) (progress * max), max);
                }
            } catch (Exception e) {
                // Likely the user isn't running Windows 7
                // or somehow has disabled the API's.
                // Ignore the exception.
                Log.Debug("Exception while setting window progress", e);
            }
        }

        private void ShowTaskPopup(ICommandTask task) {
            var dispatcher = this.taskbarIcon.Dispatcher;

            if (!dispatcher.CheckAccess()) {
                dispatcher.BeginInvoke(new Action<ICommandTask>(ShowTaskPopup), task);

                return;
            }

            task.Completed += (sender, e) => OnTaskCompleted();
            task.Canceled += (sender, e) => OnTaskCanceled();

            if (ShowNotificationOnStart) {
                this.taskPopup.Show();
            }

            if (task.State == TaskState.Completed) {
                this.taskPopup.Hide(CompleteHideDelay);
            } else if (task.State == TaskState.Canceled) {
                this.taskPopup.Hide(CancelHideDelay);
            }
        }

        private void OnTaskCompleted() {
            if (ShowNotificationOnComplete) {
                this.taskPopup.Show();
            }

            this.taskPopup.Hide(CompleteHideDelay);
        }

        private void OnTaskCanceled() {
            if (ShowNotificationOnComplete) {
                this.taskPopup.Show();
            }

            this.taskPopup.Hide(CancelHideDelay);
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
            this.logo = new NoCapLogo();

            this.taskbarIcon = InitTaskbarIcon(BuildContextMenu(pluginContext));
            this.commandRunner = InitCommandRunner(pluginContext.CommandRunner);
            this.taskCollection = InitTaskCollection(pluginContext.CommandRunner);
            this.taskPopup = InitTaskPopup(this.taskCollection);

            this.taskbarIcon.ShowCustomBalloon(this.taskPopup, PopupAnimation.None, null);

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            InitCommandBindings();

            UpdateIcon(1);
        }

        private void InitCommandBindings() {
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
                    (sender, e) => this.taskPopup.Show(),
                    (sender, e) => {
                        e.CanExecute = this.taskCollection.Count != 0;
                        e.Handled = true;
                    }
                )
            });
        }

        private static TaskbarIcon InitTaskbarIcon(ContextMenu contextMenu) {
            return new TaskbarIcon {
                Visibility = Visibility.Visible,
                DoubleClickCommand = ApplicationCommands.Properties,
                ContextMenu = contextMenu,
                LeftClickCommand = TaskbarCommands.ShowTasks,
            };
        }

        private static TaskPopup InitTaskPopup(TaskCollection taskCollection) {
            var taskPopup = new TaskPopup {
                DataContext = taskCollection.Select((t) => new TaskViewModel(t))
            };

            taskPopup.Hidden += (sender, e) => taskCollection.RemoveFinishedTasks();

            return taskPopup;
        }

        private ICommandRunner InitCommandRunner(ICommandRunner runner) {
            runner.TaskStarted   += BeginTask;
            runner.TaskCompleted += EndTask;
            runner.TaskCanceled  += CancelTask;

            return runner;
        }

        private TaskCollection InitTaskCollection(ICommandRunner runner) {
            var taskCollection = new TaskCollection();
            taskCollection.ProgressUpdated += UpdateProgress;

            runner.TaskStarted += (sender, e) => taskCollection.AddTask(e.Task);

            return taskCollection;
        }

        private static ContextMenu BuildContextMenu(IPluginContext pluginContext) {
            var commands = pluginContext.CommandProvider.StandAloneCommands.AsBindable();

            return new ContextMenu {
                ItemsSource = new CompositeCollection {
                    new MenuItem { Command = TaskbarCommands.ShowTasks, Header = "_Show Running Tasks" },
                    new Separator(),
                    new CollectionContainer { Collection = commands.Select((command) => BuildCommandMenuItem(command)) },
                    new Separator(),
                    new MenuItem { Command = ApplicationCommands.Properties, Header = "_Settings" },
                    new MenuItem { Command = ApplicationCommands.Close, Header = "E_xit" },
                }
            };
        }

        private static MenuItem BuildCommandMenuItem(ICommand command) {
            return new MenuItem {
                Command = NoCapCommands.Execute,
                CommandParameter = command,
                Header = command.Name,
            };
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
