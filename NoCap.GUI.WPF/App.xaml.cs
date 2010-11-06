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

        private void SaveSettings(ProgramSettings value) {
            // TODO Move to manager and inline
            this.settingsManager.ProgramSettings = value;

            this.settingsManager.Save();
        }

        public ProgramSettings LoadSettings() {
            // TODO Move to manager and inline
            return this.settingsManager.ProgramSettings;
        }

        private SettingsSession activeSettingsSession;

        private void Load() {
            this.taskbarIcon = (TaskbarIcon) Resources["taskbarIcon"];
            this.taskNotificationUi = new TaskNotificationUi(this.taskbarIcon, new NoCapLogo());
            this.commandRunner = new CommandRunner();
            this.settingsManager = new ProgramSettingsManager();

            this.activeSettingsSession = new SettingsSession(this.commandRunner, LoadSettings());

            this.taskNotificationUi.BindFrom(this.commandRunner);

            
            LoadBindings();

            this.activeSettingsSession.SetUp();
        }

        private void LoadBindings() {
            this.taskbarIcon.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
                (sender, e) => Shutdown(0)
            ));
            
            this.taskbarIcon.CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties,
                (sender, e) => ShowSettingsEditor()
            ));
        }

        private void ShowSettingsEditor() {
            if (this.settingsWindow != null) {
                this.settingsWindow.Show();
            }

            var clonedSettings = ProgramSettingsManager.CloneSettings(this.activeSettingsSession.Settings);

            this.settingsWindow = new SettingsWindow(clonedSettings);
            this.settingsWindow.Closed += (sender, e) => CheckSettingsEditorResult();

            this.activeSettingsSession.ShutDown();

            this.settingsWindow.ShowDialog();
        }

        private void CheckSettingsEditorResult() {
            // FIXME messy...

            if (this.settingsWindow.DialogResult == true) {
                SaveSettings(this.settingsWindow.ProgramSettings);

                this.activeSettingsSession.Dispose();
                this.activeSettingsSession = new SettingsSession(this.commandRunner, this.settingsWindow.ProgramSettings);
            }

            this.settingsWindow.Close();
            this.settingsWindow = null;

            this.activeSettingsSession.SetUp();
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
            this.activeSettingsSession.Dispose();
            this.taskNotificationUi.Dispose();
            this.taskbarIcon.Dispose();
        }
    }
}
