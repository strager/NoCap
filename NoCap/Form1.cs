using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NoCap.Destinations;
using NoCap.Sources;

namespace NoCap {
    public partial class Form1 : Form {
        private readonly DataRouter router;
        private readonly ISource source;

        public Form1() {
            InitializeComponent();

            this.source = new ScreenshotSource(ScreenshotSourceType.EntireDesktop);

            this.router = new DataRouter();
            router.Routes[TypedDataType.Image] = new FileSystemDestination(@".");
        }

        private void button1_Click(object sender, EventArgs e) {
            var sourceOp = this.source.Get();
            sourceOp.Completed += (sender2, e2) => {
                var destOp = this.router.Put(e2.Data);
                destOp.Completed += (sender3, e3) => {
                    Log("done");
                };

                destOp.Start();
            };

            sourceOp.Start();
        }

        public void Log(string message) {
            this.log.Text += message + Environment.NewLine;
        }
    }
}
