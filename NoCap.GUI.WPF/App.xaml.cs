using System;
using System.IO;
using System.Linq;
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
            var extensionManager = new ExtensionManager(Directory.CreateDirectory(Directory.GetCurrentDirectory()));

            this.configurationManager = new ConfigurationManager();
            this.settings = this.configurationManager.LoadSettings();

            bool loadCommandDefaults = false;

            if (this.settings == null) {
                this.settings = new ProgramSettings();

                loadCommandDefaults = true;
            }

            this.settings.Initialize(commandRunner, extensionManager);

            var featureRegistry = this.settings.PluginContext.FeatureRegistry;

            featureRegistry.Register(CommandFeatures.ImageUploader, "Image uploader");
            featureRegistry.Register(CommandFeatures.UrlShortener,  "Url shortener" );
            featureRegistry.Register(CommandFeatures.FileUploader,  "File uploader" );
            featureRegistry.Register(CommandFeatures.TextUploader,  "Text uploader" );

            if (loadCommandDefaults) {
                this.settings.LoadCommandDefaults();
            }
        }

        public void ShowSettings() {
            if (this.settingsWindow != null) {
                this.settingsWindow.Activate();

                return;
            }

            this.settingsWindow = new SettingsWindow(this.settings);
            this.settingsWindow.Closed += (sender, e) => SettingsClosed();
            this.settingsWindow.Show();
        }

        private void SettingsClosed() {
            this.settingsWindow = null;

            this.configurationManager.SaveSettings(this.settings);
        }

        private void Start() {
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
