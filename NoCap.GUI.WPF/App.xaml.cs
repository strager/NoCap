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

            LoadBindings();

            SetUpPlugins();
        }

        private void LoadBindings() {
        }

        internal void ShowSettingsEditor() {
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
            this.taskbarPlugin.Dispose();

            DisposePlugins();
        }
    }
}
