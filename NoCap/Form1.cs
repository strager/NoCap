using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using NoCap.Destinations;
using NoCap.Sources;

namespace NoCap {
    public partial class Form1 : Form {
        private readonly DataRouter router;
        private readonly ISource source;
        private readonly SourcePutterThing sourcePutterThing;

        public Form1() {
            InitializeComponent();

            this.source = new ScreenshotSource(ScreenshotSourceType.EntireDesktop);

            this.router = new DataRouter();
            router.Routes[DestinationType.Image] = new FileSystemDestination(@".");

            this.sourcePutterThing = new SourcePutterThing(this.router, new FormResultThing(this));
        }

        private void button1_Click(object sender, EventArgs e) {
            this.source.Get(this.sourcePutterThing);
        }

        public void Log(string message) {
            this.log.Text += message + Environment.NewLine;
        }
    }

    internal class SourcePutterThing : ISourceResultThing {
        private readonly IDestination destination;
        private readonly IResultThing resultThing;

        public SourcePutterThing(IDestination destination, IResultThing resultThing) {
            this.destination = destination;
            this.resultThing = resultThing;
        }

        public void Start() {
        }

        public void Done(DestinationType type, object data, string name) {
            this.destination.Put(type, data, name, this.resultThing);
        }

        public void Cancelled() {
        }

        public void Error(string message) {
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
