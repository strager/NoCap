using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Destinations;
using NoCap.Plugins;

namespace NoCap.GUI.WPF.Templates {
    public class ClipboardUploaderTemplate : ITemplate, INotifyPropertyChanged {
        private TextUploader textUploader;
        private UrlShortener urlShortener;
        private ImageUploader imageUploader;

        private readonly Clipboard clipboardSource;
        private readonly Clipboard clipboardDestination;

        private string name = "Clipboard uploader";

        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = name;

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

        public ClipboardUploaderTemplate() {
            this.clipboardSource = new Clipboard();
            this.clipboardDestination = new Clipboard();

            var codecs = ImageCodecInfo.GetImageEncoders().Where<ImageCodecInfo>(ImageWriter.IsCodecValid);

            ImageUploader = new ImageBinUploader(new ImageWriter(codecs.FirstOrDefault(codec => codec.FormatDescription == "PNG")));

            TextUploader = new SlexyUploader();
            UrlShortener = new IsgdShortener();
        }

        public SourceDestinationCommand GetCommand() {
            var router = new DataRouter {
                {
                    TypedDataType.Image,
                    new DestinationChain(
                        new CropShot(),
                        ImageUploader,
                        this.clipboardDestination
                        )
                    },

                {
                    TypedDataType.Text,
                    new DestinationChain(
                        TextUploader,
                        this.clipboardDestination
                        )
                    },

                {
                    TypedDataType.Uri,
                    new DestinationChain(
                        UrlShortener,
                        this.clipboardDestination
                        )
                    }
            };

            return new SourceDestinationCommand(Name, clipboardSource, router);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}