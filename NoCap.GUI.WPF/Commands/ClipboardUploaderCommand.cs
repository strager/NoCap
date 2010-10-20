using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
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

        public ICommand Clone() {
            return new ClipboardUploaderCommand {
                Name = Name,
                ImageUploader = ImageUploader,
                TextUploader = TextUploader,
                UrlShortener = UrlShortener,
            };
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

    [Export(typeof(ICommandFactory))]
    public class ClipboardUploaderCommandFactory : ICommandFactory {
        public string Name {
            get {
                return "Clipboard uploader";
            }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new ClipboardUploaderCommand {
                ImageUploader = infoStuff.GetImageUploaders().FirstOrDefault(),
                UrlShortener = infoStuff.GetUrlShorteners().FirstOrDefault(),
                TextUploader = infoStuff.GetTextUploaders().FirstOrDefault(),
            };
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new ClipboardUploaderCommandEditor((ClipboardUploaderCommand) command) {
                ImageUploaders = infoStuff.GetImageUploaders(),
                UrlShorteners = infoStuff.GetUrlShorteners(),
                TextUploaders = infoStuff.GetTextUploaders(),
            };
        }
    }
}