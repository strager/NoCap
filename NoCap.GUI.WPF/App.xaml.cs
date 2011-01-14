using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using log4net;
using log4net.Config;
using Mono.Options;
using NoCap.GUI.WPF.Runtime;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Util;
using NoCap.Library;
using NoCap.Library.Tasks;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : IDisposable {
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        private SettingsWindow settingsWindow;

        private ApplicationSettings applicationSettings;
        private ProgramSettings settings;
        private ExtensionManager extensionManager;

        private bool guiMode = true;

        private void Load() {
            var commandRunner = new CommandRunner();

            Log.Info("Loading extension manager");
            this.extensionManager = new ExtensionManager(Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Extensions")));

            Log.Info("Loading application settings");
            this.applicationSettings = new ApplicationSettings(new ProgramSettingsDataSerializer(this.extensionManager));

            ProgramSettingsData settingsData = null;

            try {
                settingsData = this.applicationSettings.LoadSettingsData();
            } catch (SerializationException e) {
                Log.Warn("Loading settings failed", e);

                if (this.guiMode) {
                    MessageBox.Show(
                        "An error occurred loading your NoCap settings.  Settings will be restored to their defaults.",
                        "Error loading settings",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation
                    );
                }
            }

            bool loadCommandDefaults = false;

            if (settingsData == null) {
                Log.Debug("Application settings not found; using defaults");

                settingsData = new ProgramSettingsData();

                loadCommandDefaults = true;
            }

            this.settings = ProgramSettings.Create(settingsData, commandRunner, this.extensionManager);

            var featureRegistry = this.settings.FeatureRegistry;

            // TODO Move elsewhere
            featureRegistry.Register(CommandFeatures.ImageUploader, "Image uploader");
            featureRegistry.Register(CommandFeatures.UrlShortener,  "Url shortener" );
            featureRegistry.Register(CommandFeatures.FileUploader,  "File uploader" );
            featureRegistry.Register(CommandFeatures.TextUploader,  "Text uploader" );

            if (loadCommandDefaults) {
                this.settings.LoadCommandDefaults();
            }
        }

        private bool TryParseArguments(IEnumerable<string> args, out int exitCode) {
            Log.Debug("Parsing command line arguments");

            bool showHelp = false;

            var optionSet = new OptionSet {
                { "x|hide", "do not show the settings window on startup", (v) => this.guiMode = v == null },
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
            if (this.guiMode) {
                ShowSettings();
            }
        }

        private void StartUpApplication(object sender, StartupEventArgs e) {
            InitializeLogging();

            Log.Info("Initializing application");

            int exitCode;

            if (!TryParseArguments(e.Args, out exitCode)) {
                Shutdown(exitCode);

                return;
            }

            Load();
            Start();

            Log.Info("Application initialized");
        }

        private static void InitializeLogging() {
            AppDomain.CurrentDomain.UnhandledException += UnhandleExceptionOccured;

            XmlConfigurator.Configure();
        }

        private static void UnhandleExceptionOccured(object sender, UnhandledExceptionEventArgs e) {
            var exception = e.ExceptionObject as Exception;

            if (exception != null) {
                Log.Error("Unhandled exception", exception);
            } else {
                Log.ErrorFormat("Unhandled unknown exception: {0}", e.ExceptionObject);
            }
        }

        private void ExitApplication(object sender, ExitEventArgs e) {
            Log.Info("Shutting down application");

            Dispose();

            Process.FlushDOPE();

            Log.Info("Application shut down");
        }

        public void Dispose() {
            this.settings.Dispose();
            this.extensionManager.Dispose();
        }
    }
}
