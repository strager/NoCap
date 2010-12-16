using System;
using System.IO;
using System.Linq;
using System.Windows;
using NoCap.GUI.WPF.Runtime;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Extensions;
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

            var settingsData = this.configurationManager.LoadSettingsData();

            bool loadCommandDefaults = false;

            if (settingsData == null) {
                settingsData = new ProgramSettingsData();

                loadCommandDefaults = true;
            }

            this.settings = ProgramSettings.Create(settingsData, commandRunner, extensionManager);

            var featureRegistry = this.settings.FeatureRegistry;

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

            this.configurationManager.SaveSettingsData(this.settings.SettingsData);
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
