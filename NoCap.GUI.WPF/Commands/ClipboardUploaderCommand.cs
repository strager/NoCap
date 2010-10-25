using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Processors;
using NoCap.Library.Util;
using NoCap.Plugins.Processors;

namespace NoCap.GUI.WPF.Commands {
    public class ClipboardUploaderCommand : HighLevelCommand, INotifyPropertyChanged {
        private IProcessor textUploader;
        private IProcessor urlShortener;
        private IProcessor imageUploader;

        private readonly Clipboard clipboardProcessor = new Clipboard();

        private string name = "Clipboard uploader";

        public override string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        public IProcessor TextUploader {
            get {
                return this.textUploader;
            }

            set {
                this.textUploader = value;

                Notify("TextUploader");
            }
        }

        public IProcessor UrlShortener {
            get {
                return this.urlShortener;
            }

            set {
                this.urlShortener = value;

                Notify("UrlShortener");
            }
        }

        public IProcessor ImageUploader {
            get {
                return this.imageUploader;
            }

            set {
                this.imageUploader = value;

                Notify("ImageUploader");
            }
        }

        public HighLevelCommand Clone() {
            return new ClipboardUploaderCommand {
                Name = Name,
                ImageUploader = ImageUploader,
                TextUploader = TextUploader,
                UrlShortener = UrlShortener,
            };
        }

        public override IProcessorFactory GetFactory() {
            return new ClipboardUploaderCommandFactory();
        }

        public override void Execute(IMutableProgressTracker progress) {
            var router = new DataRouter();

            router.Connect(TypedDataType.Image, new ProcessorChain(
                ImageUploader,
                this.clipboardProcessor
            ));

            router.Connect(TypedDataType.Text, new ProcessorChain(
                TextUploader,
                this.clipboardProcessor
            ));

            router.Connect(TypedDataType.Uri, new ProcessorChain(
                UrlShortener,
                this.clipboardProcessor
            ));

            // TODO Progress
            var clipboardData = this.clipboardProcessor.Process(null, progress);

            router.Process(clipboardData, progress);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Export(typeof(IProcessorFactory))]
    public class ClipboardUploaderCommandFactory : IProcessorFactory {
        public string Name {
            get {
                return "Clipboard uploader";
            }
        }

        public IProcessor CreateProcessor(IInfoStuff infoStuff) {
            return new ClipboardUploaderCommand {
                ImageUploader = infoStuff.GetImageUploaders().FirstOrDefault(),
                UrlShortener = infoStuff.GetUrlShorteners().FirstOrDefault(),
                TextUploader = infoStuff.GetTextUploaders().FirstOrDefault(),
            };
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor, IInfoStuff infoStuff) {
            return new ClipboardUploaderCommandEditor((ClipboardUploaderCommand) processor) {
                ImageUploaders = infoStuff.GetImageUploaders(),
                UrlShorteners = infoStuff.GetUrlShorteners(),
                TextUploaders = infoStuff.GetTextUploaders(),
            };
        }
    }
}