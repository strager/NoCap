using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using NoCap.Library;
using NoCap.Library.Processors;
using NoCap.Library.Util;
using NoCap.Plugins.Processors;

namespace NoCap.GUI.WPF.Commands {
    public class ClipboardUploaderCommand : ICommand, INotifyPropertyChanged {
        private TextUploader textUploader;
        private UrlShortener urlShortener;
        private ImageUploader imageUploader;

        private readonly Clipboard clipboardProcessor = new Clipboard();

        private string name = "Clipboard uploader";

        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        public TextUploader TextUploader {
            get {
                return this.textUploader;
            }

            set {
                this.textUploader = value;

                Notify("TextUploader");
            }
        }

        public UrlShortener UrlShortener {
            get {
                return this.urlShortener;
            }

            set {
                this.urlShortener = value;

                Notify("UrlShortener");
            }
        }

        public ImageUploader ImageUploader {
            get {
                return this.imageUploader;
            }

            set {
                this.imageUploader = value;

                Notify("ImageUploader");
            }
        }

        public ClipboardUploaderCommand() {
            var codecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            ImageUploader = new ImageBinUploader(new ImageWriter(codecs.FirstOrDefault(codec => codec.FormatDescription == "PNG")));

            TextUploader = new SlexyUploader();
            UrlShortener = new IsgdShortener();
        }

        internal ClipboardUploaderCommand(ImageUploader imageUploader, TextUploader textUploader, UrlShortener urlShortener) {
            ImageUploader = imageUploader;
            TextUploader = textUploader;
            UrlShortener = urlShortener;
        }

        public ICommand Clone() {
            return new ClipboardUploaderCommand(ImageUploader, TextUploader, UrlShortener);
        }

        public ICommandFactory GetFactory() {
            return new ClipboardUploaderCommandFactory();
        }

        public void Execute(IMutableProgressTracker progress) {
            var router = new DataRouter {
                {
                    TypedDataType.Image,
                    new ProcessorChain(
                        ImageUploader,
                        this.clipboardProcessor
                    )
                },

                {
                    TypedDataType.Text,
                    new ProcessorChain(
                        TextUploader,
                        this.clipboardProcessor
                    )
                },

                {
                    TypedDataType.Uri,
                    new ProcessorChain(
                        UrlShortener,
                        this.clipboardProcessor
                    )
                }
            };

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

    public class ClipboardUploaderCommandFactory : ICommandFactory {
        public string Name {
            get {
                return "Clipboard uploader";
            }
        }

        public ICommand CreateCommand() {
            return new ClipboardUploaderCommand();
        }

        public ICommandEditor GetCommandEditor(ICommand command) {
            return new ClipboardUploaderCommandEditor((ClipboardUploaderCommand) command);
        }
    }
}