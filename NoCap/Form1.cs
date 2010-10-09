using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NoCap.Destinations;

namespace NoCap {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            var img = ScreenCapturer.CaptureEntireDesktop();

            IImageDestination destination = new FileSystemDestination(@".");

            destination.PutImage(img, "img", new FormResultThing(this));

            MessageBox.Show("done");
        }

        public void Log(string message) {
            this.log.Text += message + Environment.NewLine;
        }
    }

    internal class FormResultThing : IResultThing {
        private readonly Form1 form;

        public FormResultThing(Form1 form) {
            this.form = form;
        }

        public void Start() {
            this.form.Log("starting");
        }

        public void Progress(float progress) {
            this.form.Log(string.Format("progress: {0}%", progress * 100));
        }

        public void Done(string reference) {
            this.form.Log("done");
        }

        public void Error(string message) {
            this.form.Log(string.Format("error: {0}", message));
        }
    }
}
