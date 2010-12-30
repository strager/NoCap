using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Mono.Options;
using NoCap.GUI.WPF.Runtime;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Util;
using NoCap.Library;
using NoCap.Library.Tasks;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : IDisposable {
        private SettingsWindow settingsWindow;

        private ApplicationSettings applicationSettings;
        private ProgramSettings settings;
        private ExtensionManager extensionManager;

        private bool showSettingsOnStart = true;

        private void Load() {
            var commandRunner = new CommandRunner();
            this.extensionManager = new ExtensionManager(Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Extensions")));

            this.applicationSettings = new ApplicationSettings(new ProgramSettingsDataSerializer(this.extensionManager));

            var settingsData = this.applicationSettings.LoadSettingsData();

            bool loadCommandDefaults = false;

            if (settingsData == null) {
                settingsData = new ProgramSettingsData();

                loadCommandDefaults = true;
            }

            this.settings = ProgramSettings.Create(settingsData, commandRunner, this.extensionManager);

            var featureRegistry = this.settings.FeatureRegistry;

            featureRegistry.Register(CommandFeatures.ImageUploader, "Image uploader");
            featureRegistry.Register(CommandFeatures.UrlShortener,  "Url shortener" );
            featureRegistry.Register(CommandFeatures.FileUploader,  "File uploader" );
            featureRegistry.Register(CommandFeatures.TextUploader,  "Text uploader" );

            if (loadCommandDefaults) {
                this.settings.LoadCommandDefaults();
            }
        }

        private bool TryParseArguments(IEnumerable<string> args, out int exitCode) {
            bool showHelp = false;

            var optionSet = new OptionSet {
                { "x|hide", "do not show the settings window on startup", (v) => this.showSettingsOnStart = v == null },
                { "h|help", "show this message and exit", (v) => showHelp = v != null },
            };

            try {
                optionSet.Parse(args);
            } catch (OptionException e) {
                Console.WriteLine("NoCap: {0}", e.Message);
                Console.WriteLine("Try `NoCap --help' for more information.");

                exitCode = 1;

                return false;
            }

            if (showHelp) {
                ShowHelp(optionSet);

                exitCode = 0;

                return false;
            }

            exitCode = 0;

            return true;
        }

        private static void ShowHelp(OptionSet optionSet) {
            Console.WriteLine("Usage: NoCap [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
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

            this.applicationSettings.SaveSettingsData(this.settings.SettingsData);
        }

        private void Start() {
            if (this.showSettingsOnStart) {
                ShowSettings();
            }
        }

        private void StartUpApplication(object sender, StartupEventArgs e) {
            int exitCode;

            if (!TryParseArguments(e.Args, out exitCode)) {
                Shutdown(exitCode);

                return;
            }

            Load();
            Start();
        }

        private void ExitApplication(object sender, ExitEventArgs e) {
            Dispose();
        }

        public void Dispose() {
            this.settings.Dispose();
            this.extensionManager.Dispose();
        }
    }
}
