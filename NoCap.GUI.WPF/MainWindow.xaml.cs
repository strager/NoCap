using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Interop;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Destinations;
using NoCap.Library.Util;
using NoCap.Plugins;
using WinputDotNet;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private readonly DataRouter router;

        private readonly ISource screenshotSource;
        private readonly ISource clipboardSource;

        private readonly IDestination clipboardDestination;

        private readonly NotifyingProgressTracker progressTracker;

        private ProgramSettings settings;

        private ProgramSettings Settings {
            get {
                return this.settings;
            }

            set {
                var oldSettings = this.settings;

                this.settings = value;

                OnSettingsChanged(oldSettings, value);
            }
        }

        public MainWindow() {
            InitializeComponent();

            DataContext = this;

            this.screenshotSource = new ScreenshotSource { SourceType = ScreenshotSourceType.EntireDesktop };
            this.clipboardSource = new Clipboard();

            this.clipboardDestination = new Clipboard();

            this.progressTracker = new NotifyingProgressTracker();
            this.progressTracker.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    SetProgress(this.progressTracker.Progress);
                }
            };

            var codecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            this.router = new DataRouter();

            router[TypedDataType.Image] = new DestinationChain(
                new CropShot(),
                new ImageBinUploader(new ImageWriter(codecs.FirstOrDefault(codec => codec.FormatDescription == "PNG"))),
                this.clipboardDestination
            );

            router[TypedDataType.Text] = new DestinationChain(
                new SlexyUploader(),
                this.clipboardDestination
            );

            router[TypedDataType.Uri] = new DestinationChain(
                new IsgdShortener(),
                this.clipboardDestination
            );

            Settings = new ProgramSettings {
                Commands = new ObservableCollection<SourceDestinationCommand>(new List<SourceDestinationCommand> {
                    new SourceDestinationCommand("Clipboard", this.clipboardSource, this.router),
                })
            };
        }

        private void OnSettingsChanged(ProgramSettings oldSettings, ProgramSettings newSettings) {
            if (oldSettings != null) {
                ShutDownEverything(oldSettings);
            }

            if (newSettings != null) {
                SetUpEverything(newSettings);
            }
        }

        private void SetUpEverything(ProgramSettings newSettings) {
            SetUpInput(newSettings);
        }

        private void ShutDownEverything(ProgramSettings oldSettings) {
            ShutDownInput(oldSettings);
        }

        private void SetUpInput(ProgramSettings newSettings) {
            var inputProvider = newSettings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            var handle = new WindowInteropHelper(this).Handle;
            
            inputProvider.CommandStateChanged += CommandStateChanged;
            inputProvider.SetBindings(newSettings.Bindings);
            inputProvider.Attach(handle);
        }

        private void ShutDownInput(ProgramSettings oldSettings) {
            var inputProvider = oldSettings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            inputProvider.Detach();
            inputProvider.CommandStateChanged -= CommandStateChanged;
        }

        private void CommandStateChanged(object sender, CommandStateChangedEventArgs e) {
            var command = (SourceDestinationCommand) e.Command;

            PerformRequestAsync(command);
        }

        private void SetProgress(double progress) {
            var setProgress = new Action(() => this.progressBar.Value = progress);

            Dispatcher.BeginInvoke(setProgress);

            Log("Progress: {0}", progress);
        }

        private void ScreenshotClicked(object sender, EventArgs e) {
            PerformRequest(this.screenshotSource);
        }

        private void ClipboardClicked(object sender, EventArgs e) {
            PerformRequest(this.clipboardSource);
        }

        private void SettingsClicked(object sender, EventArgs e) {
            var settingsWindow = new SettingsWindow(Settings.Clone());

            ShutDownInput(this.settings);

            if (settingsWindow.ShowDialog() == true) {
                Settings = settingsWindow.ProgramSettings;
            } else {
                SetUpInput(this.settings);
            }
        }

        private void PerformRequest(ISource source) {
            var routeFromAsync = new Func<ISource, IMutableProgressTracker, TypedData>(this.router.RouteFrom);

            routeFromAsync.BeginInvoke(source, this.progressTracker, (ar) => {
                System.Diagnostics.Debug.WriteLine(this.progressTracker.Progress.ToString());
                routeFromAsync.EndInvoke(ar);
            }, null);
        }

        private void PerformRequestSync(SourceDestinationCommand command, IMutableProgressTracker progress) {
            var sourceProgress = new NotifyingProgressTracker();
            var destProgress = new NotifyingProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(sourceProgress, destProgress);
            aggregateProgress.BindTo(progress);

            var data = command.Source.Get(sourceProgress);

            if (data == null) {
                return;
            }

            command.Destination.Put(data, destProgress);
        }

        private void PerformRequestAsync(SourceDestinationCommand command) {
            var func = new Action<SourceDestinationCommand, IMutableProgressTracker>(PerformRequestSync);

            func.BeginInvoke(command, this.progressTracker, func.EndInvoke, null);
        }

        private void Log(string format, params object[] args) {
            var log = new Action(() => this.messageLog.AppendText(string.Format(format, args) + Environment.NewLine));

            Dispatcher.BeginInvoke(log);
        }
    }
}
