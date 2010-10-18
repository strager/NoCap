using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using NoCap.Library;
using NoCap.Library.Destinations;
using NoCap.Library.Util;
using NoCap.Plugins;

namespace NoCap.GUI.WPF.Templates {
    public class ClipboardUploaderCommand : ICommand, INotifyPropertyChanged {
        private TextUploader textUploader;
        private UrlShortener urlShortener;
        private ImageUploader imageUploader;

        private readonly Clipboard clipboardSource = new Clipboard();
        private readonly Clipboard clipboardDestination = new Clipboard();

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

        public TypedData Get(IMutableProgressTracker progress) {
            var router = new DataRouter {
                {
                    TypedDataType.Image,
                    new DestinationChain(
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

            return router.RouteFrom(this.clipboardSource, progress);
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes() {
            return null;
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

        public ICommand CreateTemplate() {
            return new ClipboardUploaderCommand();
        }

        public ContentControl GetCommandEditor(ICommand command) {
            return new ClipboardUploaderCommandEditor();
        }
    }
}