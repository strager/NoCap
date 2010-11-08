using System;
using System.Windows;
using NoCap.GUI.WPF.Plugins;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Settings.Editors;
using NoCap.Library.Tasks;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : IDisposable {
        private SettingsWindow settingsWindow;
        private TaskbarPlugin taskbarPlugin;

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
            this.taskbarPlugin = new TaskbarPlugin(this);
            this.commandRunner = new CommandRunner();
            this.configurationManager = new ConfigurationManager();

            this.taskbarPlugin.CommandRunner = this.commandRunner;

            SetUpPlugins();
        }

        internal void ShowSettings() {
            if (this.settingsWindow != null) {
                this.settingsWindow.Activate();
            }

            this.settingsWindow = new SettingsWindow(LoadSettings());
            this.settingsWindow.Closed += (sender, e) => SettingsClosed();
            this.settingsWindow.Show();
        }

        private void SettingsClosed() {
            this.settingsWindow = null;

            SaveSettings(LoadSettings());
        }

        private void SetUpPlugins() {
            foreach (var plugin in LoadSettings().Plugins) {
                plugin.Populate(Providers.CompositionContainer);
                plugin.CommandRunner = this.commandRunner;
                plugin.Init();
            }
        }

        private void DisposePlugins() {
            foreach (var plugin in LoadSettings().Plugins) {
                plugin.Dispose();
            }
        }

        public void Start() {
            ShowSettings();
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
            this.taskbarPlugin.Dispose();

            DisposePlugins();
        }
    }
}
