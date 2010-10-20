using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Processors;
using NoCap.Plugins;
using Clipboard = NoCap.Plugins.Clipboard;

namespace NoCap {
    public partial class Form1 : Form {
        private readonly DataRouter router;

        private readonly IProcessor screenshotProcessor;

        private readonly IProcessor clipboardProcessor;

        public Form1() {
            InitializeComponent();

            this.screenshotProcessor = new Screenshot { SourceType = ScreenshotSourceType.EntireDesktop };
            this.clipboardProcessor = new Clipboard();

            var codecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            this.router = new DataRouter();

            router[TypedDataType.Image] = new ProcessorChain(
                new CropShot(),
                new ImageBinUploader(new ImageWriter(codecs.FirstOrDefault(codec => codec.FormatDescription == "PNG"))),
                this.clipboardProcessor
            );

            router[TypedDataType.Text] = new ProcessorChain(
                new SlexyUploader(),
                this.clipboardProcessor
            );

            router[TypedDataType.Uri] = new ProcessorChain(
                new IsgdShortener(),
                this.clipboardProcessor
            );
        }

        private void ScreenshotClicked(object sender, EventArgs e) {
            PerformRequest(this.screenshotProcessor);
        }

        private void ClipboardClicked(object sender, EventArgs e) {
            PerformRequest(this.clipboardProcessor);
        }

        private void PerformRequest(IProcessor source) {
            var data = this.router.Process(source.Process(null, null), null);

            Log(data.ToString());
        }

        public void Log(string message) {
            Invoke((MethodInvoker) delegate {
                this.log.Text += message + Environment.NewLine;
            });
        }
    }
}
