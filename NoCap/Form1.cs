using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Plugins.Commands;
using Clipboard = NoCap.Plugins.Commands.Clipboard;

namespace NoCap {
    public partial class Form1 : Form {
        private readonly DataRouter router;

        private readonly ICommand screenshotCommand;

        private readonly ICommand clipboardCommand;

        public Form1() {
            InitializeComponent();

            this.screenshotCommand = new Screenshot { SourceType = ScreenshotSourceType.EntireDesktop };
            this.clipboardCommand = new Clipboard();

            var codecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            this.router = new DataRouter();

            router.Connect(TypedDataType.Image, new CommandChain(
                new CropShot(),
                new ImageBinUploader(new ImageWriter(codecs.FirstOrDefault(codec => codec.FormatDescription == "PNG"))),
                this.clipboardCommand
            ));

            router.Connect(TypedDataType.Text, new CommandChain(
                new SlexyUploader(),
                this.clipboardCommand
            ));

            router.Connect(TypedDataType.Uri, new CommandChain(
                new IsgdShortener(),
                this.clipboardCommand
            ));
        }

        private void ScreenshotClicked(object sender, EventArgs e) {
            PerformRequest(this.screenshotCommand);
        }

        private void ClipboardClicked(object sender, EventArgs e) {
            PerformRequest(this.clipboardCommand);
        }

        private void PerformRequest(ICommand source) {
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
