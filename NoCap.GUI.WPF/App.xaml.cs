using System;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Settings.Editors;
using NoCap.Library;
using WinputDotNet;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        private TaskbarIcon taskbarIcon;

        private SettingsWindow settingsWindow;
        private TaskNotificationUi taskNotificationUi;

        private CommandRunner commandRunner;

        private ProgramSettingsManager settingsManager;

        public ProgramSettings Settings {
            get {
                return this.settingsManager.ProgramSettings;
            }

            private set {
                this.settingsManager.ProgramSettings = value;
            }
        }

        private void Load() {
            this.taskbarIcon = (TaskbarIcon) Resources["taskbarIcon"];
            this.taskNotificationUi = new TaskNotificationUi(this.taskbarIcon, new NoCapLogo());
            this.commandRunner = new CommandRunner();

            this.taskNotificationUi.BindFrom(this.commandRunner);

            LoadSettings();
            LoadBindings();

            SetUpEverything(Settings);
        }

        private void LoadBindings() {
            // FIXME Make close binding work!
            // This binding doesn't work (if uncommented and if matching XAML uncommented).
            // The menu item shows as disabled.
            //
            // var closeBinding = new System.Windows.Input.CommandBinding(ApplicationCommands.Close);
            // closeBinding.Executed += (sender, e) => Shutdown(0);
            // this.taskbarIcon.CommandBindings.Add(closeBinding);

            var settingsBinding = new System.Windows.Input.CommandBinding(ApplicationCommands.Properties);
            settingsBinding.Executed += (sender, e) => ShowSettingsEditor();
            this.taskbarIcon.CommandBindings.Add(settingsBinding);
        }

        private void LoadSettings() {
            this.settingsManager = new ProgramSettingsManager();
        }

        private void SetUpEverything(ProgramSettings newSettings) {
            SetUpInput(newSettings);
        }

        private void ShutDownEverything(ProgramSettings oldSettings) {
            ShutDownInput(oldSettings);
        }

        private void SetUpInput(ProgramSettings newSettings) {
            var inputProvider = newSettings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            var handle = IntPtr.Zero;
            
            inputProvider.CommandStateChanged += CommandStateChanged;
            inputProvider.SetBindings(newSettings.Bindings);
            inputProvider.Attach(handle);
        }

        private void ShutDownInput(ProgramSettings oldSettings) {
            var inputProvider = oldSettings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            inputProvider.Detach();
            inputProvider.CommandStateChanged -= CommandStateChanged;
        }

        private void CommandStateChanged(object sender, CommandStateChangedEventArgs e) {
            if (e.State == InputState.On) {
                var command = (BoundCommand) e.Command;

                this.commandRunner.Run(command.Command);
            }
        }

        private void ShowSettingsEditor() {
            if (this.settingsWindow != null) {
                this.settingsWindow.Show();
            }

            var clonedSettings = ProgramSettingsManager.CloneSettings(Settings);

            this.settingsWindow = new SettingsWindow(clonedSettings);
            this.settingsWindow.Closed += (sender, e) => CheckSettingsEditorResult();

            ShutDownInput(Settings);

            this.settingsWindow.ShowDialog();
        }

        private void CheckSettingsEditorResult() {
            if (this.settingsWindow.DialogResult == true) {
                Settings = this.settingsWindow.ProgramSettings;

                SaveSettings();
            }

            this.settingsWindow.Close();
            this.settingsWindow = null;

            SetUpInput(Settings);
        }

        private void SaveSettings() {
            this.settingsManager.Save();
        }

        public void Start() {
            ShowSettingsEditor();
        }

        private void ExitClicked(object sender, RoutedEventArgs e) {
            Shutdown(0);
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            Load();
            Start();
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            ShutDownEverything(Settings);

            this.taskbarIcon.Dispose();
            this.taskNotificationUi.Dispose();
        }
    }
}
