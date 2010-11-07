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

        private ConfigurationManager configurationManager;

        private void SaveSettings(ProgramSettings value) {
            // TODO Move to manager and inline
            this.configurationManager.ProgramSettings = value;

            this.configurationManager.Save();
        }

        public ProgramSettings LoadSettings() {
            // TODO Move to manager and inline
            return this.configurationManager.ProgramSettings;
        }

        private void Load() {
            this.taskbarIcon = (TaskbarIcon) Resources["taskbarIcon"];
            this.taskNotificationUi = new TaskNotificationUi(this.taskbarIcon, new NoCapLogo());
            this.commandRunner = new CommandRunner();
            this.configurationManager = new ConfigurationManager();

            this.taskNotificationUi.BindFrom(this.commandRunner);

            LoadBindings();

            SetUpPlugins();
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

            var clonedSettings = ConfigurationManager.CloneSettings(LoadSettings());

            this.settingsWindow = new SettingsWindow(clonedSettings);
            this.settingsWindow.Closed += (sender, e) => CheckSettingsEditorResult();

            ShutDownPlugins();

            this.settingsWindow.ShowDialog();
        }

        private void CheckSettingsEditorResult() {
            // FIXME messy...

            if (this.settingsWindow.DialogResult == true) {
                DisposePlugins();

                SaveSettings(this.settingsWindow.Settings);
            }

            this.settingsWindow.Close();
            this.settingsWindow = null;

            SetUpPlugins();
        }

        private void SetUpPlugins() {
            foreach (var plugin in LoadSettings().Plugins) {
                plugin.Populate(Providers.CompositionContainer);
                plugin.CommandRunner = this.commandRunner;
                plugin.SetUp();
            }
        }

        private void ShutDownPlugins() {
            foreach (var plugin in LoadSettings().Plugins) {
                plugin.ShutDown();
            }
        }

        private void DisposePlugins() {
            foreach (var plugin in LoadSettings().Plugins) {
                plugin.Dispose();
            }
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
            this.taskNotificationUi.Dispose();
            this.taskbarIcon.Dispose();

            DisposePlugins();
        }
    }
}
