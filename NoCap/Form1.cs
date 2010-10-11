﻿using System;
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
        private readonly ISource cropShotSource;

        public Form1() {
            InitializeComponent();

            this.screenshotSource = new ScreenshotSource(ScreenshotSourceType.EntireDesktop);
            this.clipboardSource = new ClipboardSource();
            this.cropShotSource = new CropShotSource();

            var codecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            this.router = new DataRouter();
            router.Routes[TypedDataType.Image] = new DestinationChain(new IDestination[] {
                new ImageWriter(codecs.FirstOrDefault()),
                new FileSystemDestination(@".")
            });
            /*router.Routes[TypedDataType.Image] = new ImageBinUploader(
                new ImageWriter(codecs.FirstOrDefault(codec => codec.FormatDescription == "PNG"))
            );*/
            router.Routes[TypedDataType.Text] = new SlexyUploader();
            router.Routes[TypedDataType.Uri] = new IsgdShortener();
        }

        private void ScreenshotClicked(object sender, EventArgs e) {
            PerformRequest(this.screenshotSource);
        }

        private void ClipboardClicked(object sender, EventArgs e) {
            PerformRequest(this.clipboardSource);
        }

        private void CropShotClicked(object sender, EventArgs e) {
            PerformRequest(this.cropShotSource);
        }

        private void PerformRequest(ISource source) {
            var sourceOp = source.Get();
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
            Invoke((MethodInvoker)delegate {
                this.log.Text += message + Environment.NewLine;
            });
        }
    }
}
