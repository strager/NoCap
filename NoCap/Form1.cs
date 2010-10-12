using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Destinations;
using NoCap.Library.Sources;
using NoCap.Plugins;

namespace NoCap {
    public partial class Form1 : Form {
        private readonly DataRouter router;

        private readonly ISource screenshotSource;
        private readonly ISource clipboardSource;

        private readonly IDestination clipboardDestination;

        public Form1() {
            InitializeComponent();

            this.screenshotSource = new ScreenshotSource { SourceType = ScreenshotSourceType.EntireDesktop };
            this.clipboardSource = new ClipboardSource();

            this.clipboardDestination = new ClipboardDestination();

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

        private void ScreenshotClicked(object sender, EventArgs e) {
            PerformRequest(this.screenshotSource);
        }

        private void ClipboardClicked(object sender, EventArgs e) {
            PerformRequest(this.clipboardSource);
        }

        private void PerformRequest(ISource source) {
            var op = this.router.RouteFrom(source.Get());

            op.Completed +=
                (sender, e) => Log(e.Data.ToString());

            op.Start();
        }

        public void Log(string message) {
            Invoke((MethodInvoker) delegate {
                this.log.Text += message + Environment.NewLine;
            });
        }
    }
}
