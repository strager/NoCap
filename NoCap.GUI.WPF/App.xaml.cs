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

        private ProgramSettings settings;

        private void Load() {
            this.configurationManager = new ConfigurationManager();
            this.settings = this.configurationManager.LoadSettings();

            this.commandRunner = new CommandRunner();

            SetUpPlugins();
        }

        internal void ShowSettings() {
            if (this.settingsWindow != null) {
                this.settingsWindow.Activate();
            }

            this.settingsWindow = new SettingsWindow(this.settings);
            this.settingsWindow.Closed += (sender, e) => SettingsClosed();
            this.settingsWindow.Show();
        }

        private void SettingsClosed() {
            this.settingsWindow = null;

            this.configurationManager.SaveSettings(this.settings);
        }

        private void SetUpPlugins() {
            foreach (var plugin in this.settings.Plugins) {
                plugin.Populate(Providers.CompositionContainer);
                plugin.CommandRunner = this.commandRunner;
                plugin.Init();
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
            this.settings.Dispose();
        }
    }
}
