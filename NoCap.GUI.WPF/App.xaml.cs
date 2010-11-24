using System;
using System.Windows;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Settings.Editors;
using NoCap.Library;
using NoCap.Library.Tasks;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : IDisposable {
        private SettingsWindow settingsWindow;

        private ConfigurationManager configurationManager;
        private ProgramSettings settings;

        private void Load() {
            var commandRunner = new CommandRunner();
            var extensionManager = new ExtensionManager();

            this.configurationManager = new ConfigurationManager();
            this.settings = this.configurationManager.LoadSettings();

            if (this.settings == null) {
                this.settings = ProgramSettings.LoadDefaultSettings(commandRunner, extensionManager);
            } else {
                this.settings.RuntimePluginInfo = new ProgramRuntimePluginInfo(commandRunner, extensionManager, this.settings);;
            }

            this.settings.RuntimePluginInfo.RegisterDefaultType(CommandFeatures.ImageUploader, "Image uploader");
            this.settings.RuntimePluginInfo.RegisterDefaultType(CommandFeatures.UrlShortener, "Url shortener");
            this.settings.RuntimePluginInfo.RegisterDefaultType(CommandFeatures.FileUploader, "File uploader");
            this.settings.RuntimePluginInfo.RegisterDefaultType(CommandFeatures.TextUploader, "Text uploader");

            this.settings.Plugins.Initialize(this.settings.RuntimePluginInfo);
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

        public void Start() {
            ShowSettings();
        }

        private void StartUpApplication(object sender, StartupEventArgs e) {
            Load();
            Start();
        }

        private void ExitApplication(object sender, ExitEventArgs e) {
            Dispose();
        }

        public void Dispose() {
            this.settings.Dispose();
        }
    }
}
