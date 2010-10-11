using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Linq;
using NoCap.Destinations;
using NoCap.Sources;

namespace NoCap {
    public partial class Form1 : Form {
        private readonly DataRouter router;
        private readonly ISource screenshotSource;
        private readonly ISource clipboardSource;

        public Form1() {
            InitializeComponent();

            this.screenshotSource = new ScreenshotSource(ScreenshotSourceType.EntireDesktop);
            this.clipboardSource = new ClipboardSource();

            var codecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            this.router = new DataRouter();
            router.Routes[TypedDataType.Image] = new DestinationChain(new IDestination[] {
                new ImageWriter(codecs.FirstOrDefault()),
                new FileSystemDestination(@".")
            });
            router.Routes[TypedDataType.Text] = new SlexyUploader();
        }

        private void ScreenshotClicked(object sender, EventArgs e) {
            var sourceOp = this.screenshotSource.Get();
            sourceOp.Completed += (sender2, e2) => {
                var destOp = this.router.Put(e2.Data);
                destOp.Start();

                Log(sourceOp.Data.ToString());
            };

            sourceOp.Start();
        }

        private void ClipboardClicked(object sender, EventArgs e) {
            var sourceOp = this.clipboardSource.Get();
            sourceOp.Completed += (sender2, e2) => {
                var destOp = this.router.Put(e2.Data);
                destOp.Completed += (sender3, e3) => {
                    Log(e3.Data.ToString());
                };

                destOp.Start();

                Log(sourceOp.Data.ToString());
            };

            sourceOp.Start();
        }

        public void Log(string message) {
            this.log.Text += message + Environment.NewLine;
        }
    }
}
