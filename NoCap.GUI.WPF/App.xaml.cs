using System;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Settings.Editors;
using NoCap.Library.Tasks;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : IDisposable {
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

        private SettingsSession settingsSession;

        private void Load() {
            this.taskbarIcon = (TaskbarIcon) Resources["taskbarIcon"];
            this.taskNotificationUi = new TaskNotificationUi(this.taskbarIcon, new NoCapLogo());
            this.commandRunner = new CommandRunner();

            this.settingsSession = new SettingsSession(this.commandRunner, Settings);

            this.taskNotificationUi.BindFrom(this.commandRunner);

            LoadSettings();
            LoadBindings();

            this.settingsSession.SetUp();
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

        private void ShowSettingsEditor() {
            if (this.settingsWindow != null) {
                this.settingsWindow.Show();
            }

            var clonedSettings = ProgramSettingsManager.CloneSettings(Settings);

            this.settingsWindow = new SettingsWindow(clonedSettings);
            this.settingsWindow.Closed += (sender, e) => CheckSettingsEditorResult();

            this.settingsSession.ShutDown();

            this.settingsWindow.ShowDialog();
        }

        private void CheckSettingsEditorResult() {
            if (this.settingsWindow.DialogResult == true) {
                Settings = this.settingsWindow.ProgramSettings;

                SaveSettings();
            }

            this.settingsWindow.Close();
            this.settingsWindow = null;

            this.settingsSession.SetUp();
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

        private void StartUpApplication(object sender, StartupEventArgs e) {
            Load();
            Start();
        }

        private void ExitApplication(object sender, ExitEventArgs e) {
            Dispose();
        }

        public void Dispose() {
            this.settingsSession.Dispose();
            this.taskNotificationUi.Dispose();
            this.taskbarIcon.Dispose();
        }
    }
}
