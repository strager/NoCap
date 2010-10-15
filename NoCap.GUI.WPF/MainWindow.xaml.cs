using System;
using System.ComponentModel.Composition.Hosting;
using System.Drawing.Imaging;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Destinations;
using NoCap.Library.Util;
using NoCap.Plugins;

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
            new SettingsWindow().ShowDialog();
        }

        private void PerformRequest(ISource source) {
            var routeFromAsync = new Func<ISource, IMutableProgressTracker, TypedData>(this.router.RouteFrom);

            routeFromAsync.BeginInvoke(source, this.progressTracker, (ar) => {
                System.Diagnostics.Debug.WriteLine(this.progressTracker.Progress.ToString());
                routeFromAsync.EndInvoke(ar);
            }, null);
        }

        private void Log(string format, params object[] args) {
            var log = new Action(() => this.messageLog.AppendText(string.Format(format, args) + Environment.NewLine));

            Dispatcher.BeginInvoke(log);
        }
    }
}
