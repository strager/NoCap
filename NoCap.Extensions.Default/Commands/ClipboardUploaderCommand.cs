using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Progress;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "ClipboardUploader")]
    public sealed class ClipboardUploaderCommand : HighLevelCommand, INotifyPropertyChanged, IExtensibleDataObject {
        private ICommand textUploader;
        private ICommand urlShortener;
        private ICommand imageUploader;
        private ICommand fileUploader;

        public override string Name {
            get { return "Clipboard uploader"; }
        }

        [DataMember(Name = "TextUploader")]
        public ICommand TextUploader {
            get {
                return this.textUploader;
            }

            set {
                this.textUploader = value;

                Notify("TextUploader");
            }
        }

        [DataMember(Name = "UrlShortener")]
        public ICommand UrlShortener {
            get {
                return this.urlShortener;
            }

            set {
                this.urlShortener = value;

                Notify("UrlShortener");
            }
        }

        [DataMember(Name = "ImageUploader")]
        public ICommand ImageUploader {
            get {
                return this.imageUploader;
            }

            set {
                this.imageUploader = value;

                Notify("ImageUploader");
            }
        }

        [DataMember(Name = "FileUploader")]
        public ICommand FileUploader {
            get {
                return this.fileUploader;
            }

            set {
                this.fileUploader = value;

                Notify("FileUploader");
            }
        }

        public override ICommandFactory GetFactory() {
            return new ClipboardUploaderCommandFactory();
        }

        public override ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public override bool IsValid() {
            return ImageUploader.IsValidAndNotNull()
                && TextUploader.IsValidAndNotNull()
                && UrlShortener.IsValidAndNotNull();
        }

        public override void Execute(IMutableProgressTracker progress, CancellationToken cancelToken) {
            var clipboard = new Clipboard();
            var router = new DataRouter {
                { TypedDataType.Image, ImageUploader },
                { TypedDataType.Text, TextUploader },
                { TypedDataType.Uri, UrlShortener },
                { TypedDataType.Stream, FileUploader },
            };

            var commandChain = new CommandChain(
                clipboard,
                router,
                clipboard
            );

            using (commandChain.Process(null, progress, cancelToken)) {
                // Auto-dispose
            }

            progress.Status = "URL saved to clipboard";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}